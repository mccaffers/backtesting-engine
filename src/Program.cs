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
using Report;

namespace backtesting_engine;

public static class Program
{

    public static ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public static ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject >();

    public readonly static AccountObj accountObj = new AccountObj(){
        openingEquity = decimal.Parse(EnvironmentVariables.accountEquity),
        maximumDrawndownPercentage = decimal.Parse(EnvironmentVariables.maximumDrawndownPercentage)
    };

    // Dependency Injection Scope
    public readonly static IServiceScope scope = RegisterServices().CreateScope();

    public static async Task Main(string[] args) {

        try{
            await PullDataFromS3();
        } catch(Exception ex){
            
            System.Console.WriteLine(ex);
            // If anything fails let elastic know
            Reporting.SendStack(ex);
        }

    }

    private static async Task PullDataFromS3(){
        var s3Path = EnvironmentVariables.s3Path;
        var s3bucket = EnvironmentVariables.s3Bucket;

        foreach(var year in EnvironmentVariables.years){
            foreach(var symbol in EnvironmentVariables.symbols){
                
                var symbolFolder = Path.Combine(EnvironmentVariables.tickDataFolder, symbol);
                var command = "mkdir -p " + symbolFolder;
                await ShellHelper.Bash(command);
                
                // Don't pull from S3 if it already exists, useful for local testing
                if(!File.Exists("./tickdata/" + symbol + "/"+year+".csv")){
                    command = "aws s3api get-object --bucket "+s3bucket+" --key "+s3Path+symbol+"/"+year+".csv.zst "+symbolFolder + "/"+year+".csv.zst";
                    await ShellHelper.Bash(command);
                    command = "zstd -d -f --rm ./tickdata/"+symbol+"/*.zst";
                    await ShellHelper.Bash(command);
                }
                await new Main().IngestAndConsume(new Consumer(), new Ingest());
                command = "rm -rf " + symbolFolder + "/*";
            }
        }
    }

    

    private static ServiceProvider RegisterServices()
    {

        // Define the services to inject
        var services = new ServiceCollection();

        foreach(var i in EnvironmentVariables.strategy.Split(",")){

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
