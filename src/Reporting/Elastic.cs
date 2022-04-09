
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using trading_exception;
using Utilities;

namespace Reporting;

public interface IElastic
{
    List<ReportTradeObj> tradeUpdateArray { get; init; }

    Task BatchTradeUpdate();
    Task EndOfRunReport(string reason = "");
    Task SendBatchedObjects(List<ReportTradeObj> localClone);
    Task SendStack(TradingException message);
    Task TradeUpdate(DateTime date, string symbol, decimal profit);
}

public class Elastic : TradingBase, IElastic
{
    public DateTime lastPostTime {get; set;} = DateTime.Now;
    public List<ReportTradeObj> tradeUpdateArray { get; init; }
    public bool switchHasSentFinalReport { get; set; }

    private IElasticClient elasticClient;

    public Elastic(IServiceProvider provider, IElasticClient elasticClient) : base(provider)
    {
        this.tradeUpdateArray = new List<ReportTradeObj>();
        this.elasticClient = elasticClient;
    }

    public async Task EndOfRunReport(string reason = "")
    {
        if (!EnvironmentVariables.reportingEnabled || switchHasSentFinalReport)
        {
            return;
        }

        switchHasSentFinalReport = true;

        var positivePercentage = 0;
        if (this.tradingObjects.tradeHistory.Any(x => x.Value.profit > 0))
        {
            positivePercentage = (this.tradingObjects.tradeHistory.Count(x => x.Value.profit > 0) / this.tradingObjects.tradeHistory.Count(x => x.Value.profit < 0)) * 100;
        }

        var report = new ReportFinalObj()
        {
            date = DateTime.Now,
            hostname = EnvironmentVariables.hostname,
            symbols = EnvironmentVariables.symbols,
            pnl = this.tradingObjects.accountObj.pnl,
            runID = EnvironmentVariables.runID,
            runIteration = int.Parse(EnvironmentVariables.runIteration),
            openingEquity = this.tradingObjects.accountObj.openingEquity,
            maximumDrawndownPercentage = this.tradingObjects.accountObj.maximumDrawndownPercentage,
            strategy = EnvironmentVariables.strategy,
            positiveTradeCount = this.tradingObjects.tradeHistory.Count(x => x.Value.profit > 0),
            negativeTradeCount = this.tradingObjects.tradeHistory.Count(x => x.Value.profit < 0),
            positivePercentage = positivePercentage,
            systemRunTimeInMinutes = DateTime.Now.Subtract(this.systemObjects.systemStartTime).TotalMinutes,
        };

        if (!this.tradingObjects.tradeHistory.IsEmpty)
        {
            report.tradingTimespanInDays = this.tradingObjects.tradeTime.Subtract(this.tradingObjects.tradeHistory.First().Value.openDate).TotalDays;
        }

        if (!string.IsNullOrEmpty(reason))
        {
            report.complete = false;
            report.reason = reason;
        }

        // Make sure we send all of the trading objects
        await SendBatchedObjects(tradeUpdateArray);

        var response = await elasticClient.IndexAsync(report, b => b.Index("report"));
        System.Console.WriteLine(response);

        // Give the requests enough time to clean up, probably 
        // not necessary with the above await operators
        System.Threading.Thread.Sleep(5000);
    }

    public async Task SendStack(TradingException message)
    {
        if (!EnvironmentVariables.reportingEnabled)
        {
            return;
        }

        await elasticClient.IndexAsync(message, b => b.Index("exception"));
        System.Threading.Thread.Sleep(5000);
    }

    public async Task TradeUpdate(DateTime date, string symbol, decimal profit)
    {
        tradeUpdateArray.Add(new ReportTradeObj()
        {
            date = date,
            symbols = EnvironmentVariables.symbols,
            pnl = this.tradingObjects.accountObj.pnl,
            runID = EnvironmentVariables.runID,
            runIteration = int.Parse(EnvironmentVariables.runIteration),
            tradeProfit = profit
        });
        await BatchTradeUpdate();
    }

    public async Task BatchTradeUpdate()
    {
        if (!EnvironmentVariables.reportingEnabled)
        {
            return;
        }

        if (DateTime.Now.Subtract(lastPostTime).TotalSeconds <= 5)
        {
            return;
        }

        lastPostTime = DateTime.Now;
        List<ReportTradeObj> localClone = new List<ReportTradeObj>(tradeUpdateArray);
        await SendBatchedObjects(localClone); // don't wait (await) for this, let it process in the background
    }

    public async Task SendBatchedObjects(List<ReportTradeObj> localClone)
    {
        // Upload the trade results
        await elasticClient.BulkAsync(bd => bd.IndexMany(localClone, (descriptor, s) => descriptor.Index("trades")));

        // Clear the history
        tradeUpdateArray.RemoveAll(x => localClone.Any(y => y.id == x.id));
    }
}

