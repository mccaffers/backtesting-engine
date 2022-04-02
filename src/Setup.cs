using Reporting;
using trading_exception;
using Utilities;

namespace backtesting_engine;

public class SystemSetup : ISystemSetup
{
    public SystemSetup(ITaskManager main)
    {

        Task.Run(async () =>
        {
            try {
                await StartEngine(main);
            } catch (Exception ex) {
                System.Console.WriteLine(ex);
                TradingException myEx = new TradingException(ex.Message, ex);
                await Elastic.EndOfRunReport(myEx.Message);
                await Elastic.SendStack(myEx); // report error to elastic for review
            }
            await Elastic.EndOfRunReport("EndOfBuffer");
        }).Wait();
    }

    public virtual async Task StartEngine(ITaskManager main)
    {
        foreach (var year in EnvironmentVariables.years)
        {
            foreach (var symbol in EnvironmentVariables.symbols)
            {

                var symbolFolder = Path.Combine(EnvironmentVariables.tickDataFolder, symbol);
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
            await CleanSymbolFolder(EnvironmentVariables.tickDataFolder);
        }
    }

    private static async Task Decompress(string symbol)
    {
        var command = "zstd -d -f --rm ./tickdata/" + symbol + "/*.zst";
        await ShellHelper.Bash(command);
    }

    private static async Task PullFromS3(string symbolFolder, string symbol, int year)
    {
        var s3Path = EnvironmentVariables.s3Path;
        var s3bucket = EnvironmentVariables.s3Bucket;

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
