
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Utilities;

namespace Report;

public static class Reporting
{

    static CloudConnectionPool pool = new CloudConnectionPool(EnvironmentVariables.elasticCloudID, new BasicAuthenticationCredentials(EnvironmentVariables.elasticUser,EnvironmentVariables.elasticPassword));
    
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2));

    static ElasticClient esClient = new ElasticClient(settings);

    private static DateTime lastPostTime = DateTime.Now;
    private static List<ReportObj> tradeUpdateArray = new List<ReportObj>();

    public static void EndOfRunReport(string reason){
         if(!EnvironmentVariables.reportingFlag){
            return;
        }

        var report = new ReportObj(){
            date = DateTime.Now,
            symbols= EnvironmentVariables.symbols,
            pnl=Program.accountObj.pnl,
            runID=EnvironmentVariables.runID,
            openingEquity=Program.accountObj.openingEquity,
            maximumDrawndownPercentage=Program.accountObj.maximumDrawndownPercentage,
            strategy=EnvironmentVariables.strategy,
            status="complete",
            reason=reason
        };
        esClient.Index(report,b=>b.Index("report"));
    }

    public static void SendStack(Exception message){
         if(!EnvironmentVariables.reportingFlag){
            return;
        }
        
        esClient.Index(message,b=>b.Index("exception"));
    }


    public static void TradeUpdate(DateTime date, string symbol, decimal profit){
         tradeUpdateArray.Add(new ReportObj(){
            date=date,
            symbols=EnvironmentVariables.symbols,
            pnl=Program.accountObj.pnl,
            runID=EnvironmentVariables.runID,
            openingEquity=Program.accountObj.openingEquity,
            maximumDrawndownPercentage=Program.accountObj.maximumDrawndownPercentage,
            strategy=EnvironmentVariables.strategy
        });
        BatchTradeUpdate();
    }

    private static void BatchTradeUpdate(){
        if(!EnvironmentVariables.reportingFlag){
            return;
        }

        if(DateTime.Now.Subtract(lastPostTime).TotalSeconds <= 5 ){
            return;
        }

        lastPostTime=DateTime.Now;
        
        // Upload the trade results
        esClient.Bulk(bd => bd.IndexMany(tradeUpdateArray, (descriptor, s) => descriptor.Index("trades")));
                
        // Clear the history
        tradeUpdateArray.RemoveRange(0, tradeUpdateArray.Count);
    }

}

