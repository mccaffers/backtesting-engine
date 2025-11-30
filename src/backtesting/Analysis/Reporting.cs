using System.Net;
using Nest;
using trading_exception;
using Utilities;

namespace backtesting_engine.analysis;

public interface IReporting
{
    List<ReportTradeObj> tradeUpdateArray { get; init; }

    Task BatchTradeUpdate();
    void EndOfRunReport(string reason = "");
    Task SendBatchedObjects(List<ReportTradeObj> localClone, int retry = 0);
    Task SendStack(TradingException message);
    void TradeUpdate(DateTime date, string symbol, decimal profit, decimal spread, double tradeDuration);
}

public class Reporting : TradingBase, IReporting
{
    public DateTime lastPostTime { get; set; } = DateTime.Now;
    public List<ReportTradeObj> tradeUpdateArray { get; init; }

    private readonly Nest.IElasticClient elasticClient;
    private readonly decimal stopDistanceInPips;
    private readonly decimal limitDistanceInPips;
    private readonly int runIteration;

    public Reporting(IServiceProvider provider, Nest.IElasticClient elasticClient) : base(provider)
    {
        this.tradeUpdateArray = new List<ReportTradeObj>();
        this.elasticClient = elasticClient;

        stopDistanceInPips = decimal.Parse(TradingVariables.STOP_DISTANCE_IN_PIPS.Value());
        limitDistanceInPips = decimal.Parse(TradingVariables.LIMIT_DISTANCE_IN_PIPS.Value());
        runIteration = int.Parse(BACKTESTING.RUN_ITERATION.Value());
    }

    public async Task<string> SendStackException(Exception ex)
    {
        await SendStack(new TradingException(ex.Message, ex)); // report error to elastic for review
        return ex.Message;
    }

    private static int lastMonths = int.Parse(BACKTESTING.LAST_MONTHS.Value());

    public void EndOfRunReport(string reason = "")
    {
        if (!bool.Parse(LoggingVariables.REPORT_TO_ELASTICSEARCH.Value()))
        {
            return;
        }

        // Made no trades, no need to report
        if (tradingObjects.tradeHistory.Count == 0)
        {
            ConsoleLogger.SystemLog("Abandoning run due to no trades");
            return;
        }

        // Only proceed with reporting if we should report losses AND there are actually losses
        if (tradingObjects.accountObj.pnl < tradingObjects.accountObj.openingEquity && !bool.Parse(BACKTESTING.REPORT_LOSSES.Value()))
        {
            ConsoleLogger.SystemLog("Abandoning run due to loss: " + tradingObjects.accountObj.pnl);
            ConsoleLogger.SystemLog($"System Run Time (Minutes): {DateTime.Now.Subtract(this.systemObjects.systemStartTime).TotalMinutes:N2}");
            return;
        }

        var winRate = 0m;

        var negativeTrades = Convert.ToDecimal(this.tradingObjects.tradeHistory.Count(x => x.Value.profit < 0));
        var positiveTrades = Convert.ToDecimal(this.tradingObjects.tradeHistory.Count(x => x.Value.profit > 0));
        var totalSum = positiveTrades + negativeTrades;

        if (negativeTrades == 0)
        {
            winRate = 1;
        }
        else if (positiveTrades != 0 && totalSum != 0)
        {
            winRate = positiveTrades / totalSum;
        }

        var complete = true;
        var message = "";
        if (!string.IsNullOrEmpty(reason))
        {
            complete = false;
            message = reason;
        }

        // Make sure we send all of the trading objects
        if (tradeUpdateArray.Count > 0 && (bool.Parse(BACKTESTING.REPORT_INDIVIDUAL_TRADES.Value()) == true))
        {
            elasticClient.IndexMany(tradeUpdateArray, "trades");
        }

        var environmentReport = ReportEnvironmentVariables.Get();

        var tradeRatio = 0m;
        if (positiveTrades > 0 && negativeTrades > 0)
        {
            tradeRatio = (limitDistanceInPips * positiveTrades) / (stopDistanceInPips * negativeTrades);
        }

        if (positiveTrades == 0)
        {
            tradeRatio = 0;
        }
        if (negativeTrades == 0)
        {
            tradeRatio = 100;
        }

        // If the trade ratio is below the minimum and we are abandoning these parameters
        if (tradeRatio < decimal.Parse(BACKTESTING.MIN_TRADE_RATIO.Value()) && !bool.Parse(BACKTESTING.REPORT_LOSSES.Value()))
        {
            ConsoleLogger.SystemLog("Abandoning run due to low trade ratio: " + tradeRatio);
            return;
        }

        var performance = (tradeRatio * 0.3m) + (winRate * 0.4m) + ((1 / (tradingObjects.accountObj.accountPercentDrop + 0.0001m)) * 0.3m);

        // Get the hostname for later use
        var hostname = Dns.GetHostName();

        // If we are running a 24 month backtest, we want to report
        if (Program.BACKTESTING_VARIABLES?.LAST_MONTHS == 24)
        {
            if (tradingObjects.tradeHistory.Count > 0)
            {
                var tradeHistoryDoubles = this.tradingObjects.tradeHistory.Select(x => Convert.ToDouble(x.Value.profit)).ToArray();
                double confidenceLevel = 0.95;
                var stdDevOutput = StandardDeviationConfidenceIntervalCalculator.Calculate(tradeHistoryDoubles, confidenceLevel);
                environmentReport.Add("STD_DEV_MEAN", stdDevOutput.mean);
                environmentReport.Add("STD_DEV_CRITICAL", stdDevOutput.criticalValue);
                environmentReport.Add("STD_DEV_VALUE", stdDevOutput.stdDev);
            }

            environmentReport.Add("PNL", this.tradingObjects.accountObj.pnl);
            environmentReport.Add("POSITIVE_TRADES", positiveTrades);
            if (Program.BACKTESTING_STRATEGY_DEFINITION is not null)
            {
                environmentReport.Add("STRATEGYDEF", Program.BACKTESTING_STRATEGY_DEFINITION);
            }
            environmentReport.Add("SYMBOLS", BACKTESTING.SYMBOLS.Value());
            environmentReport.Add("NEGATIVE_TRADES", negativeTrades);
            environmentReport.Add("WIN_RATE", winRate);
            environmentReport.Add("SYSTEM_RUN_TIME", DateTime.Now.Subtract(this.systemObjects.systemStartTime).TotalMinutes);
            environmentReport.Add("COMPLETE", complete);
            environmentReport.Add("PERCENT_ACCOUNT_LOSS", tradingObjects.accountObj.accountPercentDrop);
            environmentReport.Add("REASON", message);
            environmentReport.Add("PERFORMANCE", performance);
            environmentReport.Add("RUN_ID", BACKTESTING.RUN_ID.Value());
            environmentReport.Add("HOSTNAME", hostname);
            environmentReport.Add("LAST_MONTHS", BACKTESTING.LAST_MONTHS.Value());
            environmentReport.Add("RUN_ITERATION", int.Parse(BACKTESTING.RUN_ITERATION.Value()));
            environmentReport.Add("DATE_TO", BACKTESTING.DATE_TO.Value());

            if (tradingObjects.tradeHistory.Count > 0)
            {
                environmentReport.Add("AVERAGE_TRADE_DURATION", this.tradingObjects.tradeHistory.Average(x => x.Value.runningTime));
                environmentReport.Add("MAX_TRADE_DURATION", this.tradingObjects.tradeHistory.Max(x => x.Value.runningTime));
                environmentReport.Add("MEDIAN_TRADE_DURATION", this.tradingObjects.tradeHistory.Median(x => x.Value.runningTime));
            }

            environmentReport.Add("TRADE_RATIO", tradeRatio);

            elasticClient.Index(environmentReport, b => b.Index("report"));
        }
            // Log everything to screen instead
            ConsoleLogger.SystemLog("--- END OF RUN REPORT ----");
            // SYMBOLS BACKTESTING.SYMBOLS.Value())
            ConsoleLogger.SystemLog($"Symbols: {BACKTESTING.SYMBOLS.Value()}");
            ConsoleLogger.SystemLog($"PNL: {this.tradingObjects.accountObj.pnl}");
            ConsoleLogger.SystemLog($"Positive Trades: {positiveTrades}");
            ConsoleLogger.SystemLog($"Negative Trades: {negativeTrades}");
            ConsoleLogger.SystemLog($"Win Rate: {winRate:P2}");
            ConsoleLogger.SystemLog($"Percent Account Loss: {tradingObjects.accountObj.accountPercentDrop:P2}");
            ConsoleLogger.SystemLog($"Trade Ratio: {tradeRatio:P2}");
            ConsoleLogger.SystemLog($"Performance: {performance:P2}");
            ConsoleLogger.SystemLog($"System Run Time (Minutes): {DateTime.Now.Subtract(this.systemObjects.systemStartTime).TotalMinutes:N2}");
            ConsoleLogger.SystemLog($"Complete: {complete}");
            if (!complete)
                ConsoleLogger.SystemLog($"Reason: {message}");
            ConsoleLogger.SystemLog("--------------------------");
        

        if (Program.BACKTESTING_VARIABLES is not null)
        {

            if (performance < 1m)
                return;

            Program.BACKTESTING_VARIABLES.RUN_ITERATION += 1;
        }

        Thread.Sleep(50);
    }

    public virtual async Task SendStack(TradingException message)
    {

        if (bool.Parse(LoggingVariables.REPORT_TO_ELASTICSEARCH.Value()) == false)
        {
            return;
        }

        await elasticClient.IndexAsync(message, b => b.Index("exception"));
        Thread.Sleep(300);
    }

    public void TradeUpdate(DateTime date, string symbol, decimal profit, decimal spread, double tradeDuration)
    {
        if (bool.Parse(BACKTESTING.REPORT_INDIVIDUAL_TRADES.Value()) == false)
        {
            return;
        }

        var tradeReport = new ReportTradeObj()
        {
            date = date,
            SYMBOLS = BACKTESTING.SYMBOLS.Value().Split(","),
            PNL = this.tradingObjects.accountObj.pnl,
            RUN_ID = BACKTESTING.RUN_ID.Value(),
            RUN_ITERATION = runIteration,
            PROFIT = profit,
            STOP_DISTANCE_IN_PIPS = stopDistanceInPips,
            LIMIT_DISTANCE_IN_PIPS = limitDistanceInPips,
            TRAILING_STOP_LOSS_VALUE = decimal.Parse(TradingVariables.TRAILING_STOP_LOSS_VALUE.Value()),
            // INSTANCE_COUNT = int.Parse(BACKTESTING.INSTANCE_COUNT.Value()),
            SPREAD = spread,
            TRADE_DURATION = tradeDuration,
            STRATEGY = TradingVariables.STRATEGY.Value()
        };

        tradeUpdateArray.Add(tradeReport);

        _ = BatchTradeUpdate();
    }

    public async Task BatchTradeUpdate()
    {
        if (bool.Parse(LoggingVariables.REPORT_TO_ELASTICSEARCH.Value()) == false
            || DateTime.Now.Subtract(lastPostTime).TotalSeconds <= 5)
        {
            return;
        }

        // Record we've posted to elastic
        lastPostTime = DateTime.Now;

        // Create a local copy of the list at this point in time
        List<ReportTradeObj> localClone = new List<ReportTradeObj>(tradeUpdateArray);

        // Don't wait (await) for this, let it process in the background
        await SendBatchedObjects(localClone);
    }

    public async Task SendBatchedObjects(List<ReportTradeObj> localClone, int retry = 0)
    {

        if (retry == 3)
        {
            return;
        }

        // Clear the history
        tradeUpdateArray.RemoveAll(x => localClone.Any(y => y.id == x.id));

        var bulkResponse = await elasticClient.BulkAsync(bd => bd.IndexMany(localClone, (descriptor, s) => descriptor.Index("trades")));
        if (bulkResponse != null && bulkResponse.IsValid)
        {
            // ConsoleLogger.SystemLog("Success [bulkasync] from ElasticSearch Count:" + localClone.Count + " Retry:" + retry);
        }
        else
        {
            // ConsoleLogger.SystemLog("Failure [bulkasync] from ElasticSearch Count:" + localClone.Count + " Retry:" + retry);
            _ = SendBatchedObjects(localClone, retry + 1);
        }
    }
}

