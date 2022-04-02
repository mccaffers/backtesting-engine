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
using backtesting_engine_operations;

namespace backtesting_engine;

public class Program
{

    async static Task Main(string[] args) =>
        await Task.FromResult(
            new ServiceCollection()
            .RegisterStrategies()
            .AddSingleton<IOpenOrder, OpenOrder>()
            .AddSingleton<ICloseOrder, CloseOrder>()
            .AddSingleton<IIngest, Ingest>()
            .AddSingleton<IConsumer, Consumer>()
            .AddSingleton<IPositions, Positions>()
            .AddSingleton<ITaskManager, TaskManager>()
            .AddSingleton<ISystemSetup, SystemSetup>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<IRequestOpenTrade, RequestOpenTrade>()
            .BuildServiceProvider(true)
            .CreateScope()
            .ServiceProvider.GetRequiredService<ISystemSetup>())
        .ContinueWith(task=>{
            System.Console.WriteLine("Finished");
        });


}

public static class ServiceExtension {

    public static IServiceCollection RegisterStrategies(this IServiceCollection services)
    {
        // Define the services to inject
        // var services = ;

        foreach(var i in EnvironmentVariables.strategy.Split(",")){

            var _type = Type.GetType("backtesting_engine_strategies." + i) ?? default(Type);

            // Verfiy the strategy can be created
            if(_type is not null && typeof(IStrategy).IsAssignableFrom(_type) ){
                services.AddSingleton(typeof(IStrategy), _type);
            }
        }

        if(services.Count == 0){
            throw new ArgumentException("No Strategies Found");
        }

        // Keep a record of the ServiceProvider to call it in the Trade Class
        return services; //IServiceScope
    }
}

