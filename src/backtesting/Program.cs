using backtesting_engine_ingest;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_operations;
using backtesting_engine.interfaces;
using Nest;
using Elasticsearch.Net;
using backtesting_engine.analysis;
using System.Net;
using Webserver.Api.Controllers;
using Microsoft.AspNetCore.SignalR;
using Webserver.Api.Hubs.Clients;
using Newtonsoft.Json;
using Webserver.Api.Hubs;

namespace backtesting_engine;

public class WebNotification : IWebNotification
{
    private decimal lastClose = decimal.Zero;
    private decimal count = 0;
    private decimal lastAccountBalance=0;
    private decimal accountCount=0;
    private DateTime lastSent = DateTime.Now;
    private DateTime lastSentAccount = DateTime.Now;

    public WebNotification(){
    }

    public async Task AccountUpdate(decimal input)
    {

        if(DateTime.Now.Subtract(lastSentAccount).TotalMilliseconds < 600){
            return;
        }
        lastSentAccount = DateTime.Now;

        // accountCount++;  
        // if(accountCount>5){
        //     accountCount=0;
        // }    
        // if(accountCount>1){
        //     return;
        // }       

        try {
            await Webserver.Api.Program.hubContext.Clients.All.ReceiveMessage(new Webserver.Api.Models.ChatMessage(){
                Activity="account",
                Content=input.ToString()
            });
        } 
        catch(Exception hubEx){
            System.Console.WriteLine(hubEx);
        }
    }

    
    public async Task Message(OhlcObject input, bool force = false)
    {

        if(DateTime.Now.Subtract(lastSent).TotalMilliseconds < 500 && !force){
            return;
        }
        lastSent=DateTime.Now;

        if(lastClose == input.close && !force){
            return;
        }
        lastClose=input.close;

        // count++;
        // if(count>3){
        //     count=0;
        // }
        // if(count>1 && !force){
        //     return;
        // }

        try {
            await Webserver.Api.Program.hubContext.Clients.All.ReceiveMessage(new Webserver.Api.Models.ChatMessage(){
                Activity="trade",
                Content=JsonConvert.SerializeObject(input)
            });
        }
        catch(Exception hubEx){
            System.Console.WriteLine(hubEx);
        }
    }
}

public class EmptyWebNotification : IWebNotification
{
    public EmptyWebNotification(){
    }

    public Task AccountUpdate(decimal input)
    {
        throw new NotImplementedException();
    }

    public Task Message(OhlcObject input, bool force)
    {
        return Task.CompletedTask;
    }
}

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
            
            Thread.Sleep(5000); // wait for web server to boot up
            serviceCollection.AddSingleton<IWebNotification,WebNotification>();

        } else {
            System.Console.WriteLine("Not web");
            serviceCollection.AddSingleton<IWebNotification,EmptyWebNotification>();

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
            .AddTransient<IConsumer, Consumer>()
            .AddTransient<IPositions, Positions>()
            .AddTransient<ITaskManager, TaskManager>()
            .AddTransient<ISystemSetup, SystemSetup>()
            .AddSingleton<IReporting, Reporting>()
            .AddTransient<IRequestOpenTrade, RequestOpenTrade>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IEnvironmentVariables>(variables);
            
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
            },
            TaskContinuationOptions.OnlyOnRanToCompletion
        );
    }
}


