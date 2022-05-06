using backtesting_engine_ingest;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_operations;
using backtesting_engine.interfaces;
using Nest;
using Elasticsearch.Net;
using backtesting_engine.analysis;
using System.Net;

namespace backtesting_engine;

static class Program
{

    private static EnvironmentVariables variables = new EnvironmentVariables();
    static CloudConnectionPool pool = new CloudConnectionPool(variables.elasticCloudID, new BasicAuthenticationCredentials(variables.elasticUser, variables.elasticPassword));
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2)).EnableApiVersioningHeader();

    async static Task Main(string[] args) => 
        await Task.FromResult(
            new ServiceCollection()
            .RegisterStrategies(variables)
             .AddSingleton<IElasticClient>( (IServiceProvider provider) => { 
                var esClient = new ElasticClient(settings);
                if(!esClient.Ping().IsValid && !Dns.GetHostName().Contains(".local")){
                    throw new ArgumentException("ElasticSearch settings are not valid");
                }
                return esClient;
            })
            .AddTransient<IOpenOrder, OpenOrder>()
            .AddTransient<ICloseOrder, CloseOrder>()
            .AddTransient<IIngest, backtesting_engine_ingest.Ingest>()
            .AddTransient<IConsumer, Consumer>()
            .AddTransient<IPositions, Positions>()
            .AddTransient<ITaskManager, TaskManager>()
            .AddTransient<ISystemSetup, SystemSetup>()
            .AddSingleton<IReporting, Reporting>()
            .AddTransient<IRequestOpenTrade, RequestOpenTrade>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IEnvironmentVariables>(variables)
            .BuildServiceProvider(true)
            .CreateScope()
            .ServiceProvider.GetRequiredService<ISystemSetup>())
        .ContinueWith(task=>{
            ConsoleLogger.Log("Trading run finished");
        });
}


