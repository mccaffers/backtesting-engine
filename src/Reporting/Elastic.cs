
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

    static int id = 0;

    private static void BatchTradeUpdate(){

        TimeSpan diff = DateTime.Now.Subtract(lastPostTime);
        if(diff.TotalSeconds <= 5 ){
            return;
        }
        lastPostTime=DateTime.Now;
        
        // var response = esClient.IndexMany(tradeUpdateArray,"trade");
        esClient.Bulk(bd => bd.IndexMany(tradeUpdateArray.ToArray(), (descriptor, s) => descriptor.Index("trades").Id(++id)));

        // System.Console.WriteLine(JsonConvert.SerializeObject(tradeUpdateArray));
        System.Console.WriteLine(tradeUpdateArray.Count());
        tradeUpdateArray.RemoveRange(0, tradeUpdateArray.Count());
    }

    public static void Send(){

    }

}

