
using backtesting_engine;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;

namespace Report;

public static class Reporting
{

    static ConnectionSettings settings = new ConnectionSettings("My_deployment:ZXUtd2VzdC0yLmF3cy5jbG91ZC5lcy5pbyQ5MTA5YjY1MzU0MDg0ZjIzYmM2MGZkNzEwZGMxMDdjNiQ3Yzc1OWY4NzNmMDY0MDczOGIxZDU3YTgxMGM0YTgyYQ==", new BasicAuthenticationCredentials("elastic","zP3vb1BPfXYaCyVz47GYjS6L"));
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

