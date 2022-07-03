using System.Diagnostics.CodeAnalysis;
using System.Net;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using trading_exception;
using Utilities;

namespace backtesting_engine;

public class SystemSetup : ISystemSetup
{
    readonly IEnvironmentVariables envVariables;
    readonly IReporting elastic;
    readonly IElasticClient es;
    readonly IServiceProvider provider;

    public SystemSetup(IServiceProvider provider, IReporting elastic, IElasticClient es, IEnvironmentVariables envVariables)
    {
        this.envVariables = envVariables;
        this.elastic = elastic;
        this.provider = provider;
        this.es = es;

        Task<string>.Run(async () =>
        {
            try {
                return await StartEngine();
            }
            catch (TradingException tradingException) {
                return tradingException.Message;
            }
            catch (Exception ex)
            {
                return await SendStackException(ex);
            }

        }).ContinueWith(taskOutput => {
            ConsoleLogger.Log(taskOutput.Result);
            elastic.EndOfRunReport(taskOutput.Result);
        }).Wait();
    }

    public async Task<string> SendStackException(Exception ex)
    {
        await elastic.SendStack(new TradingException(ex.Message, ex, this.envVariables)); // report error to elastic for review
        return ex.Message;
    }

    public virtual async Task<string> StartEngine()
    {
        foreach (var year in envVariables.years)
        {
            foreach (var symbol in envVariables.symbols)
            {

                var symbolFolder = Path.Combine(envVariables.tickDataFolder, symbol);
                var csvFile = Path.Combine(symbolFolder, year + ".csv");

                // Check if file already exists
                if (!File.Exists(csvFile))
                {
                    PullFromS3(symbolFolder, symbol, year);
                    Decompress(symbol);
                }
            }

            // Start Backtesting Engine, on demand
            using (var scope = this.provider.CreateScope())
            {
                var transientService = scope.ServiceProvider.GetRequiredService<ITaskManager>();
                await transientService.IngestAndConsume();
            }

            // Clean up this year of data
            if(!envVariables.doNotCleanUpDataFolder){
                CleanSymbolFolder(envVariables.tickDataFolder);
            }
            UpdateElastic(year);
        }
        return string.Empty;
    }

    private void UpdateElastic(int year){
        //Send an initial report to ElasticSearch
        es.Index(new YearUpdate() {
                    hostname = Dns.GetHostName(),
                    date = DateTime.Now,
                    symbols = envVariables.symbols,
                    runID = envVariables.runID,
                    runIteration = int.Parse(envVariables.runIteration),
                    strategy = envVariables.strategy,
                    instanceCount = envVariables.instanceCount,
                    year = year
                }, b => b.Index("yearupdate"));
    }

    private static void Decompress(string symbol)
    {
        ConsoleLogger.Log("Decompressing - " + symbol);
        var command = "zstd -d -f --rm ./tickdata/" + symbol + "/*.zst";
        ShellHelper.RunCommandWithBash(command);
    }

    private void PullFromS3(string symbolFolder, string symbol, int year)
    {
        ConsoleLogger.Log("Pulling tick data from S3 - " + symbol);

        var s3Path = envVariables.s3Path;
        var s3bucket = envVariables.s3Bucket;

        // Setup folder space for tick data
        var command = "mkdir -p " + symbolFolder;
        ShellHelper.RunCommandWithBash(command);

        command = "aws s3api get-object --bucket " + s3bucket + " --key " + s3Path + "/" + symbol + "/" + year + ".csv.zst " + symbolFolder + "/" + year + ".csv.zst";
        ShellHelper.RunCommandWithBash(command);
    }

    private static void CleanSymbolFolder(string symbolFolder)
    {
        // Delete procssed data to free up space
        var command = "rm -rf " + symbolFolder + "/*";
        ShellHelper.RunCommandWithBash(command);
    }
}
