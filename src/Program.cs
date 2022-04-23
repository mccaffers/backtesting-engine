using backtesting_engine_ingest;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_operations;
using backtesting_engine.interfaces;
using Nest;
using Elasticsearch.Net;
using backtesting_engine.analysis;

namespace backtesting_engine;

static class Program
{

    private static EnvironmentVariables variables = new EnvironmentVariables();
    static CloudConnectionPool pool = new CloudConnectionPool(variables.elasticCloudID, new BasicAuthenticationCredentials(variables.elasticUser, variables.elasticPassword));
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2));

    async static Task Main(string[] args) => 
        await Task.FromResult(
            new ServiceCollection()
            .RegisterStrategies(variables)
            .AddSingleton<IOpenOrder, OpenOrder>()
            .AddSingleton<ICloseOrder, CloseOrder>()
            .AddSingleton<IIngest, backtesting_engine_ingest.Ingest>()
            .AddSingleton<IConsumer, Consumer>()
            .AddSingleton<IPositions, Positions>()
            .AddSingleton<ITaskManager, TaskManager>()
            .AddSingleton<ISystemSetup, SystemSetup>()
            .AddSingleton<IReporting,Reporting>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IEnvironmentVariables>(variables)
            .AddSingleton<IRequestOpenTrade, RequestOpenTrade>()
            .AddSingleton<IElasticClient>(provider => new ElasticClient(settings))
            .BuildServiceProvider(true)
            .CreateScope()
            .ServiceProvider.GetRequiredService<ISystemSetup>())
        .ContinueWith(task=>{
            ConsoleLogger.Log("Finished");
        });
}


