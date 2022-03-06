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

namespace backtesting_engine;

public static class Program
{

    public static ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public static ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject >();

    public readonly static AccountObj accountObj = new AccountObj();
    public readonly static EnvironmentVariables env = new EnvironmentVariables();

    // Dependency Injection Scope
    public readonly static IServiceScope scope = RegisterServices().CreateScope();

    public static async Task Main(string[] args)
    {
        Program.accountObj.openingEquity = decimal.Parse(env.Get("accountEquity"));
        Program.accountObj.maximumDrawndownPercentage = decimal.Parse(env.Get("maximumDrawndownPercentage"));

        await new Main().IngestAndConsume(new Consumer(), new Ingest());
    }

    private static ServiceProvider RegisterServices()
    {

        // Define the services to inject
        var services = new ServiceCollection();

        foreach(var i in env.Get("strategy").Split(",")){

            var _type = Type.GetType("backtesting_engine_strategies." + i);

            // Double check the class name is correct
            if(_type==null){
                continue;
            }

            var instance = Activator.CreateInstance(_type);

            // Verfiy the strategy can be created
            if(instance==null){
                continue;
            }

            services.AddScoped<IStrategy>(serviceProvider =>
            {
                return (IStrategy)instance;
            });
        }

        if(services.Count == 0){
            throw new ArgumentException("No Strategies Found");
        }

        // Keep a record of the ServiceProvider to call it in the Trade Class
        return services.BuildServiceProvider(true); //IServiceScope
    }
}
