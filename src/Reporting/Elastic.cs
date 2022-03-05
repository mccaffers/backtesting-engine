
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Utilities;

namespace Report;

public class Reporting
{

    static EnvironmentVariables env { get; } = new EnvironmentVariables();

    // static ConnectionSettings settings = new ConnectionSettings(env.Get("elasticCloudID"), 
    //                                         new BasicAuthenticationCredentials(env.Get("elasticUser"),env.Get("elasticPassword")));

    static CloudConnectionPool pool = new CloudConnectionPool(env.Get("elasticCloudID"), new BasicAuthenticationCredentials(env.Get("elasticUser"),env.Get("elasticPassword")));
    
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2));

    static ElasticClient esClient = new ElasticClient(settings);

    private static DateTime lastPostTime = DateTime.Now;
    private static List<ReportObj> tradeUpdateArray = new List<ReportObj>();

    public static void EndOfRunReport(string reason){
        var report = new ReportObj(){
            symbols= env.Get("symbols").Split(","),
            pnl=Program.accountObj.pnl,
            runID=env.Get("runID"),
            openingEquity=Program.accountObj.openingEquity,
            maximumDrawndownPercentage=Program.accountObj.maximumDrawndownPercentage,
            strategy=env.Get("strategy"),
            status="complete",
            reason=reason
        };
        esClient.Index(report,b=>b.Index("report"));
    }

    public static void TradeUpdate(string symbol, decimal profit){
         tradeUpdateArray.Add(new ReportObj(){
            symbols= env.Get("symbols").Split(","),
            pnl=Program.accountObj.pnl,
            runID=env.Get("runID"),
            openingEquity=Program.accountObj.openingEquity,
            maximumDrawndownPercentage=Program.accountObj.maximumDrawndownPercentage,
            strategy=env.Get("strategy")
        });
        BatchTradeUpdate();
    }

    private static void BatchTradeUpdate(){

        if(DateTime.Now.Subtract(lastPostTime).TotalSeconds <= 5 ){
            return;
        }

        lastPostTime=DateTime.Now;
        
        // Upload the trade results
        var indexManyResponse = esClient.Bulk(bd => bd.IndexMany(tradeUpdateArray, (descriptor, s) => descriptor.Index("trades")));
                
        // Clear the history
        tradeUpdateArray.RemoveRange(0, tradeUpdateArray.Count());
    }

    public static void Send(){

    }

}

