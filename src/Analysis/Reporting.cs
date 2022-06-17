
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using trading_exception;
using Utilities;

namespace backtesting_engine.analysis;

public interface IReporting
{
    List<ReportTradeObj> tradeUpdateArray { get; init; }

    Task BatchTradeUpdate();
    void EndOfRunReport(string reason = "");
    Task SendBatchedObjects(List<ReportTradeObj> localClone, int retry=0);
    Task SendStack(TradingException message);
    void TradeUpdate(DateTime date, string symbol, decimal profit);
}

public class Reporting : TradingBase, IReporting
{
    public DateTime lastPostTime {get; set;} = DateTime.Now;
    public List<ReportTradeObj> tradeUpdateArray { get; init; }
    public bool switchHasSentFinalReport { get; set; }

    private readonly IElasticClient elasticClient;
    private readonly IEnvironmentVariables envVariables;

    public Reporting(IServiceProvider provider, IElasticClient elasticClient, IEnvironmentVariables envVariables) : base(provider)
    {
        this.tradeUpdateArray = new List<ReportTradeObj>();
        this.elasticClient =  elasticClient;
        this.envVariables = envVariables;
    }

    public void EndOfRunReport(string reason = "")
    {
        if (!envVariables.reportingEnabled || switchHasSentFinalReport)
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
            hostname = envVariables.hostname,
            symbols = envVariables.symbols,
            pnl = this.tradingObjects.accountObj.pnl,
            runID = envVariables.runID,
            runIteration = int.Parse(envVariables.runIteration),
            openingEquity = this.tradingObjects.accountObj.openingEquity,
            maximumDrawndownPercentage = this.tradingObjects.accountObj.maximumDrawndownPercentage,
            strategy = envVariables.strategy,
            positiveTradeCount = this.tradingObjects.tradeHistory.Count(x => x.Value.profit > 0),
            negativeTradeCount = this.tradingObjects.tradeHistory.Count(x => x.Value.profit < 0),
            positivePercentage = positivePercentage,
            systemRunTimeInMinutes = DateTime.Now.Subtract(this.systemObjects.systemStartTime).TotalMinutes,
            systemMessage = this.systemObjects.systemMessage,
            stopDistanceInPips =  int.Parse(envVariables.stopDistanceInPips),
            limitDistanceInPips =  int.Parse(envVariables.limitDistanceInPips),
            instanceCount = envVariables.instanceCount,
            environmentVariables=envVariables
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
        if(tradeUpdateArray.Count > 0){
            elasticClient.IndexMany(tradeUpdateArray, "trades");
        }
        elasticClient.Index(report, b => b.Index("report"));

        // Give the requests enough time to clean up, probably 
        // not necessary with the above await operators
        System.Threading.Thread.Sleep(3000);
    }

    public virtual async Task SendStack(TradingException message)
    {
        if (!envVariables.reportingEnabled)
        {
            return;
        }

        await elasticClient.IndexAsync(message, b => b.Index("exception"));
        System.Threading.Thread.Sleep(1000);
    }

    public void TradeUpdate(DateTime date, string symbol, decimal profit)
    {
        tradeUpdateArray.Add(new ReportTradeObj()
        {
            date = date,
            symbols = envVariables.symbols,
            pnl = this.tradingObjects.accountObj.pnl,
            runID = envVariables.runID,
            runIteration = int.Parse(envVariables.runIteration),
            tradeProfit = profit,
            stopDistanceInPips = int.Parse(envVariables.stopDistanceInPips),
            limitDistanceInPips = int.Parse(envVariables.limitDistanceInPips),
            kineticLimit=envVariables.kineticLimit,
            kineticStopLoss=envVariables.kineticStopLoss,
            instanceCount=envVariables.instanceCount
        });
        
        _=BatchTradeUpdate();
    }

    public async Task BatchTradeUpdate()
    {
        if (!envVariables.reportingEnabled || DateTime.Now.Subtract(lastPostTime).TotalSeconds <= 5)
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

    public async Task SendBatchedObjects(List<ReportTradeObj> localClone, int retry=0)
    {

        if(retry == 3){
            return;
        }

        // Clear the history
        tradeUpdateArray.RemoveAll(x => localClone.Any(y => y.id == x.id));

        var bulkResponse = await elasticClient.BulkAsync(bd => bd.IndexMany(localClone, (descriptor, s) => descriptor.Index("trades")));
        if(bulkResponse.IsValid){
            ConsoleLogger.SystemLog("Success [bulkasync] from ElasticSearch Count:" + localClone.Count + " Retry:" + retry);
        } else if(!bulkResponse.IsValid){
            ConsoleLogger.SystemLog("Failure [bulkasync] from ElasticSearch Count:" + localClone.Count + " Retry:" + retry);
            _ = SendBatchedObjects(localClone, retry+1);
        } 
    }
}

