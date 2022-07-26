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
                //Send an initial report to ElasticSearch
                esClient.Index(new InitialReport() {
                                hostname = Dns.GetHostName(),
                                date = DateTime.Now,
                                symbols = variables.symbols,
                                runID = variables.runID,
                                runIteration = int.Parse(variables.runIteration),
                                strategy = variables.strategy,
                                instanceCount = variables.instanceCount
                            }, b => b.Index("init"));
                return esClient;
            })
            .AddTransient<IOpenOrder, OpenOrder>()
            .AddSingleton<ICloseOrder, CloseOrder>()
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
        .ContinueWith(task => {
                if(task.IsFaulted){
                    Console.WriteLine(task.Exception?.Message);
                }
                if(task.IsCanceled){
                    System.Console.WriteLine("Task cancelled");
                }
                if(task.IsCompletedSuccessfully){
                    Console.WriteLine("Task completed successfully"); 
                }
            },
            TaskContinuationOptions.OnlyOnRanToCompletion
        );
}


