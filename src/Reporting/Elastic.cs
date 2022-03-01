
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Utilities;

namespace Report;

public static class Reporting
{

    static EnvironmentVariables env { get; } = new EnvironmentVariables();

    static ConnectionSettings settings = new ConnectionSettings(env.Get("elasticCloudID"), 
                                            new BasicAuthenticationCredentials(env.Get("elasticUser"),env.Get("elasticPassword")));

    static ElasticClient esClient = new ElasticClient(settings);

    public static void EndOfRunReport(AccountObj account){
        esClient.Index(account,b=>b.Index("report"));
    }

    public static void Post<T>(T input){
        // esClient.Index(input);
    }

    public static void Send(){

    }

}

