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

    public static ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public static ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject >();
    
    public static DateTime systemStartTime {get;} = DateTime.Now;
    public static DateTime tradeTime {get;set;}

    public static string systemMessage {get;set;} = "";

    public readonly static AccountObj accountObj = new AccountObj(){
        openingEquity = decimal.Parse(EnvironmentVariables.accountEquity),
        maximumDrawndownPercentage = decimal.Parse(EnvironmentVariables.maximumDrawndownPercentage)
    };
    
    // Dependency Injection Scope
    public readonly static IServiceScope scope = RegisterServices().CreateScope();

    public static async Task Main(string[] args) {

        try{
            await new Program().StartEngine();
        } catch(Exception ex){

            System.Console.WriteLine(ex);

            if(ex is TradingException tradingException){
                await Elastic.EndOfRunReport(tradingException.Message);
            } else {
                // If a unexpected exception gets throw, lets wrap it as a trading exception    
                // which contributes system datetime
                TradingException myEx = new TradingException(ex.Message, ex);
                await Elastic.SendStack(myEx); // send to elastic
                await Elastic.EndOfRunReport("Unknown system exception, see ex log");
            }

            return;
            
        }
           
        await Elastic.EndOfRunReport("EndOfBuffer");
        
    }

    private async Task StartEngine(){

        foreach(var year in EnvironmentVariables.years){
            foreach(var symbol in EnvironmentVariables.symbols){

                var symbolFolder = Path.Combine(EnvironmentVariables.tickDataFolder, symbol);
                var csvFile = Path.Combine(symbolFolder, year+".csv");
                
                // Check if file already exists
                if(!File.Exists(csvFile)){
                    await PullFromS3(symbolFolder, symbol, year);
                    await Decompress(symbol);
                }
            }
            // Start Backtesting Engine
            await new Main().IngestAndConsume(new Consumer(), new Ingest());

            // Clean up
            await CleanSymbolFolder(EnvironmentVariables.tickDataFolder);
        }
    }

    private static async Task Decompress(string symbol){
        var command = "zstd -d -f --rm ./tickdata/"+symbol+"/*.zst";
        await ShellHelper.Bash(command);
    }

    private static async Task PullFromS3(string symbolFolder, string symbol, int year){
        var s3Path = EnvironmentVariables.s3Path;
        var s3bucket = EnvironmentVariables.s3Bucket;

        // Setup folder space for tick data
        var command = "mkdir -p " + symbolFolder;
        await ShellHelper.Bash(command);

        command = "aws s3api get-object --bucket "+s3bucket+" --key "+s3Path+"/"+symbol+"/"+year+".csv.zst "+symbolFolder + "/"+year+".csv.zst";
        await ShellHelper.Bash(command);
    }

    private static async Task CleanSymbolFolder(string symbolFolder){
        // Delete procssed data to free up space
        var command = "rm -rf " + symbolFolder + "/*";
        await ShellHelper.Bash(command);
    }

    private static ServiceProvider RegisterServices()
    {
        // Define the services to inject
        var services = new ServiceCollection();

        foreach(var i in EnvironmentVariables.strategy.Split(",")){

            var _type = Type.GetType("backtesting_engine_strategies." + i);

            // Verfiy the strategy can be created
            if(Activator.CreateInstance(_type) is IStrategy strategyInstance){
                services.AddScoped<IStrategy>(serviceProvider =>
                {
                    return strategyInstance;
                });
            }
        }

        if(services.Count == 0){
            throw new ArgumentException("No Strategies Found");
        }

        // Keep a record of the ServiceProvider to call it in the Trade Class
        return services.BuildServiceProvider(true); //IServiceScope
    }
}
