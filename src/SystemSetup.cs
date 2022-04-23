using System.Diagnostics.CodeAnalysis;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using trading_exception;
using Utilities;

namespace backtesting_engine;

public class SystemSetup : ISystemSetup
{
    readonly IEnvironmentVariables envVariables;
    readonly IReporting elastic;

    public SystemSetup(ITaskManager main, IReporting elastic, IEnvironmentVariables envVariables)
    {
        this.envVariables = envVariables;
        this.elastic = elastic;

        Task<string>.Run(async () =>
        {
            try
            {
                return await StartEngine(main, envVariables);
            }
            catch (TradingException tradingException)
            {
                return tradingException.Message;
            }
            catch (Exception ex)
            {
                return await SendStackException(ex.Message);
            }

        }).ContinueWith(async taskOutput => {
            ConsoleLogger.Log(taskOutput.Result);
            await elastic.EndOfRunReport(taskOutput.Result);
        }).Wait();
    }

    public async Task<string> SendStackException(string message)
    {
        await elastic.SendStack(new TradingException(message)); // report error to elastic for review
        return message;
    }


    public virtual async Task<string> StartEngine(ITaskManager main, IEnvironmentVariables envVariables)
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
                    await PullFromS3(symbolFolder, symbol, year);
                    await Decompress(symbol);
                }
            }
            // Start Backtesting Engine
            await main.IngestAndConsume();

            // Clean up
            await CleanSymbolFolder(envVariables.tickDataFolder);
        }
        return string.Empty;
    }

    private static async Task Decompress(string symbol)
    {
        ConsoleLogger.Log("Decompressing - " + symbol);
        var command = "zstd -d -f --rm ./tickdata/" + symbol + "/*.zst";
        await ShellHelper.Bash(command);
    }

    private async Task PullFromS3(string symbolFolder, string symbol, int year)
    {
        ConsoleLogger.Log("Pulling tick data from S3 - " + symbol);

        var s3Path = envVariables.s3Path;
        var s3bucket = envVariables.s3Bucket;

        // Setup folder space for tick data
        var command = "mkdir -p " + symbolFolder;
        await ShellHelper.Bash(command);

        command = "aws s3api get-object --bucket " + s3bucket + " --key " + s3Path + "/" + symbol + "/" + year + ".csv.zst " + symbolFolder + "/" + year + ".csv.zst";
        await ShellHelper.Bash(command);
    }

    private static async Task CleanSymbolFolder(string symbolFolder)
    {
        // Delete procssed data to free up space
        var command = "rm -rf " + symbolFolder + "/*";
        await ShellHelper.Bash(command);
    }
}
