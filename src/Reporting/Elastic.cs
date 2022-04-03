
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
    Task EndOfRunReport(string reason);
    Task SendStack(TradingException message);
    void TradeUpdate(DateTime date, string symbol, decimal profit);
}

public class Elastic : TradingBase, IElastic
{
    public Elastic(IServiceProvider provider) : base(provider) { }

    static CloudConnectionPool pool = new CloudConnectionPool(EnvironmentVariables.elasticCloudID, new BasicAuthenticationCredentials(EnvironmentVariables.elasticUser, EnvironmentVariables.elasticPassword));
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2));
    static ElasticClient esClient = new ElasticClient(settings);

    private DateTime lastPostTime = DateTime.Now;
    private List<ReportTradeObj> tradeUpdateArray = new List<ReportTradeObj>();

    public async Task EndOfRunReport(string reason)
    {

        if (!EnvironmentVariables.reportingEnabled)
        {
            return;
        }

        var report = new ReportFinalObj()
        {
            date = DateTime.Now,
            hostname = EnvironmentVariables.hostname,
            symbols = EnvironmentVariables.symbols,
            pnl = this.tradingObjects.accountObj.pnl,
            runID = EnvironmentVariables.runID,
            openingEquity = this.tradingObjects.accountObj.openingEquity,
            maximumDrawndownPercentage = this.tradingObjects.accountObj.maximumDrawndownPercentage,
            strategy = EnvironmentVariables.strategy,
            positiveTradeCount = this.tradingObjects.tradeHistory.Count(x => x.Value.profit > 0),
            negativeTradeCount = this.tradingObjects.tradeHistory.Count(x => x.Value.profit < 0),
            positivePercentage = (this.tradingObjects.tradeHistory.Count(x => x.Value.profit > 0) / this.tradingObjects.tradeHistory.Count(x => x.Value.profit < 0)) * 100,
            systemRunTimeInMinutes = DateTime.Now.Subtract(this.systemObjects.systemStartTime).TotalMinutes,
        };

        if (!this.tradingObjects.tradeHistory.IsEmpty)
        {
            report.tradingTimespanInDays = this.tradingObjects.tradeTime.Subtract(this.tradingObjects.tradeHistory.First().Value.openDate).TotalDays;
        }

        if (reason == "EndOfBuffer")
        {
            report.complete = true;
        }
        else
        {
            report.complete = false;
            report.reason = reason;
        }

        await esClient.IndexAsync(report, b => b.Index("report"));
        System.Threading.Thread.Sleep(5000);
    }

    public async Task SendStack(TradingException message)
    {
        if (!EnvironmentVariables.reportingEnabled)
        {
            return;
        }

        await esClient.IndexAsync(message, b => b.Index("exception"));
        System.Threading.Thread.Sleep(5000);
    }

    public void TradeUpdate(DateTime date, string symbol, decimal profit)
    {
        tradeUpdateArray.Add(new ReportTradeObj()
        {
            date = date,
            symbols = EnvironmentVariables.symbols,
            pnl = this.tradingObjects.accountObj.pnl,
            runID = EnvironmentVariables.runID,
            tradeProfit = profit
        });
        BatchTradeUpdate();
    }

    private void BatchTradeUpdate()
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

        // Upload the trade results
        esClient.BulkAsync(bd => bd.IndexMany(tradeUpdateArray, (descriptor, s) => descriptor.Index("trades")));

        // Clear the history
        tradeUpdateArray.RemoveRange(0, tradeUpdateArray.Count);
    }

}

