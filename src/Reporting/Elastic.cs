
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using trading_exception;
using Utilities;

namespace Reporting;

public static class Elastic
{
    static CloudConnectionPool pool = new CloudConnectionPool(EnvironmentVariables.elasticCloudID, new BasicAuthenticationCredentials(EnvironmentVariables.elasticUser,EnvironmentVariables.elasticPassword));
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2));
    static ElasticClient esClient = new ElasticClient(settings);

    private static DateTime lastPostTime = DateTime.Now;
    private static List<ReportTradeObj> tradeUpdateArray = new List<ReportTradeObj>();

    public static async Task EndOfRunReport(string reason){
        
         if(!EnvironmentVariables.reportingEnabled){
            return;
        }

        var report = new ReportFinalObj(){
            date = DateTime.Now,
            hostname = EnvironmentVariables.hostname,
            symbols= EnvironmentVariables.symbols,
            pnl=Program.accountObj.pnl,
            runID=EnvironmentVariables.runID,
            openingEquity=Program.accountObj.openingEquity,
            maximumDrawndownPercentage=Program.accountObj.maximumDrawndownPercentage,
            strategy=EnvironmentVariables.strategy,
            positiveTradeCount=Program.tradeHistory.Count(x=>x.Value.profit>0),
            negativeTradeCount=Program.tradeHistory.Count(x=>x.Value.profit<0),
            positivePercentage=(Program.tradeHistory.Count(x=>x.Value.profit>0)/Program.tradeHistory.Count(x=>x.Value.profit<0))*100,
            systemRunTimeInMinutes=DateTime.Now.Subtract(Program.systemStartTime).TotalMinutes,
        };

        if(Program.tradeHistory.Count>0){
            report.tradingTimespanInDays=Program.tradeTime.Subtract(Program.tradeHistory.First().Value.openDate).TotalDays;
        }

        if(reason=="EndOfBuffer"){
            report.complete = true;
        } else {
            report.complete = false;
            report.reason = reason;
        }
        
        await esClient.IndexAsync(report,b=>b.Index("report"));
        System.Threading.Thread.Sleep(5000);
    }

    public static async Task SendStack(TradingException message){
         if(!EnvironmentVariables.reportingEnabled){
            return;
        }
        
        await esClient.IndexAsync(message,b=>b.Index("exception"));
        System.Threading.Thread.Sleep(5000);
    }

    public static void TradeUpdate(DateTime date, string symbol, decimal profit){
         tradeUpdateArray.Add(new ReportTradeObj(){
            date=date,
            symbols=EnvironmentVariables.symbols,
            pnl=Program.accountObj.pnl,
            runID=EnvironmentVariables.runID,
            tradeProfit=profit
        });
        BatchTradeUpdate();
    }

    private static void BatchTradeUpdate(){
        if(!EnvironmentVariables.reportingEnabled){
            return;
        }

        if(DateTime.Now.Subtract(lastPostTime).TotalSeconds <= 5 ){
            return;
        }

        lastPostTime=DateTime.Now;
        
        // Upload the trade results
        esClient.BulkAsync(bd => bd.IndexMany(tradeUpdateArray, (descriptor, s) => descriptor.Index("trades")));
                
        // Clear the history
        tradeUpdateArray.RemoveRange(0, tradeUpdateArray.Count);
    }

}

