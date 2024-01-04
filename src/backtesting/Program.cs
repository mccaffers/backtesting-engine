using backtesting_engine_ingest;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_operations;
using backtesting_engine.interfaces;
using Nest;
using Elasticsearch.Net;
using backtesting_engine.analysis;
using System.Net;
using backtesting_engine_web;
using System.Diagnostics;

namespace backtesting_engine;

public static class Program
{
    private static EnvironmentVariables variables = new EnvironmentVariables();
    static CloudConnectionPool pool = new CloudConnectionPool(variables.elasticCloudID, new BasicAuthenticationCredentials(variables.elasticUser, variables.elasticPassword));
    static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2)).EnableApiVersioningHeader();

    public async static Task Main(string[] args) {
  
        IServiceCollection? serviceCollection=null;
        
        if(serviceCollection == null){
            serviceCollection = new ServiceCollection();
        }

        if(args.Count() > 0 && args[0] == "web"){
            System.Console.WriteLine("Web");
            
            _ = Task.Run(() => {
                Webserver.Api.Program.Main(new string[1]{"web"});
            }).ContinueWith(task => {
                    if(task.IsFaulted){
                        Console.WriteLine(task.Exception?.Message);
                        Console.WriteLine(task.Exception?.InnerException);

                    }
                    if(task.IsCanceled){
                        System.Console.WriteLine("Task cancelled");
                    }
                    if(task.IsCompletedSuccessfully){
                        Console.WriteLine("Task completed successfully"); 
                    }
                    System.Console.WriteLine("Web Server closed");
                    Environment.Exit(0);
                }   
            );
            
            Thread.Sleep(2000); // wait for web server to boot up
            serviceCollection.AddSingleton<IWebUtils, WebUtils>();
            serviceCollection.AddSingleton<IWebNotification, WebNotification>();
            serviceCollection.AddTransient<IConsumer, WebConsumer>();
        } else {
            System.Console.WriteLine("Not web");
            serviceCollection.AddSingleton<IWebNotification,EmptyWebNotification>();
            serviceCollection.AddSingleton<IWebUtils, WebUtilsMock>();
            serviceCollection.AddTransient<IConsumer, Consumer>();
        }

        serviceCollection
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
            .AddTransient<IPositions, Positions>()
            .AddTransient<ITaskManager, TaskManager>()
            .AddTransient<ISystemSetup, SystemSetup>()
            .AddSingleton<IReporting, Reporting>()
            .AddTransient<IRequestOpenTrade, RequestOpenTrade>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IEnvironmentVariables>(variables);
    
        Stopwatch sw = new Stopwatch();
        sw.Start();

        await Task.FromResult(
            serviceCollection
            .BuildServiceProvider(true)
            .CreateScope()
            .ServiceProvider.GetRequiredService<ISystemSetup>())
        .ContinueWith(task => {
                if(task.IsFaulted){
                    Console.WriteLine(task.Exception?.Message);
                    Console.WriteLine(task.Exception?.StackTrace);
                }
                if(task.IsCanceled){
                    System.Console.WriteLine("Task cancelled");
                }
                if(task.IsCompletedSuccessfully){
                    Console.WriteLine("Task completed successfully"); 
                }

                sw.Stop();

                Console.WriteLine("Elapsed={0}",sw.Elapsed);
            }
        );
    }
}


