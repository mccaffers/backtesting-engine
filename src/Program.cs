using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_strategies;
using Reporting;
using trading_exception;

namespace backtesting_engine;

public class Program
{

    async static Task Main(string[] args) =>
        await Task.FromResult(
            RegisterStrategies()
            .AddSingleton<IIngest, Ingest>()
            .AddSingleton<IConsumer, Consumer>()
            .AddSingleton<ITaskManager, TaskManager>()
            .AddSingleton<ISystemSetup, SystemSetup>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .BuildServiceProvider(true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ISystemSetup>())
        .ContinueWith(task=>{
            System.Console.WriteLine("Finished");
        });

    private static ServiceCollection RegisterStrategies()
    {
        // Define the services to inject
        var services = new ServiceCollection();

        foreach(var i in EnvironmentVariables.strategy.Split(",")){

            var _type = Type.GetType("backtesting_engine_strategies." + i) ?? default(Type);

            // Verfiy the strategy can be created
            if(_type is not null && Activator.CreateInstance(_type) is IStrategy strategyInstance){
                services.AddSingleton<IStrategy>(serviceProvider =>
                {
                    return strategyInstance;
                });
            }
        }

        if(services.Count == 0){
            throw new ArgumentException("No Strategies Found");
        }

        // Keep a record of the ServiceProvider to call it in the Trade Class
        return services; //IServiceScope
    }
}

