using backtesting_engine;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_operations;
using Npgsql;
using trading_exception;
using Utilities;

namespace backtesting_engine_ingest;

public class IngestFromQuestDB : IIngest
{
    private static string GetPostgresConnection()
    {
        return $@"
            Host=localhost;
            Port=8812;
            Username=admin;
            Password=quest;
            Database=qdb;
            ServerCompatibilityMode=NoTypeLoading;
            Timeout=3;
            CommandTimeout=3;
            MaxPoolSize=64;
            Pooling=true";
    }

    private static readonly int last_months = int.Parse(BACKTESTING.LAST_MONTHS.Value());
    private static readonly NpgsqlDataSource dataSource = NpgsqlDataSource.Create(GetPostgresConnection());

    readonly IStrategy strategy;
    readonly IPositions positions;
    readonly IReporting elastic;

    public void EnvironmentSetup() { }

    private static string dateToString = BACKTESTING.DATE_TO.Value();

    // Fast, lightweight constructor
    public IngestFromQuestDB(IEnumerable<IStrategy> strategies,
                           IReporting elastic,
                           IPositions positions)
    {
        this.strategy = strategies.First();
        this.positions = positions;
        this.elastic = elastic;

        DateTime.TryParse(dateToString, out var dateToParsed);
        if(dateToParsed == DateTime.MinValue){
            dateToString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
        }
    }

    // Explicit initialization method
    public async Task ExecuteAsync()
    {
        string result = string.Empty;
        
        try
        {
            await ReadLines();
            ConsoleLogger.Log("Data ingestion completed successfully");
        }
        catch (TradingException tradingException)
        {
            result = tradingException.Message;
            ConsoleLogger.Log($"Trading error: {result}");
            if (!result.Contains("MAX_OVERALL_DRAWDOWN_THRESHOLD"))
            {
                await SendStackException(tradingException);
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            Console.WriteLine(ex);
            ConsoleLogger.Log($"Unexpected error: {result}");
            await SendStackException(ex);
          
        }
        finally
        {
            // Cleanup operations
            await CleanupAsync(result);
        }
    }

    private async Task CleanupAsync(string result)
    {
        try
        {
            positions.CloseAll();
            
            // Use async delay instead of Thread.Sleep
            await Task.Delay(100);
            
            elastic.EndOfRunReport(result);
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log($"Error during cleanup: {ex.Message}");
        }
    }

    public async Task<string> SendStackException(Exception ex)
    {
        await elastic.SendStack(new TradingException(ex.Message, ex)); // report error to elastic for review
        return ex.Message;
    }

    private static readonly string symbol = BACKTESTING.SYMBOLS.Value();
    private static readonly int size = 50000;

    public async Task ReadLines()
    {
        var (hasMore, lastDate) = await Request();
        while (hasMore)
        {
            (hasMore, lastDate) = await Request(lastDate);
        }
        await strategy.Exit();
    }


  private async Task<(bool, DateTime)> Request(DateTime? from = null)
    {
        var dateFrom = from is null ? $"dateadd('M', -{last_months}, now())" : $"'{from:yyyy-MM-ddTHH:mm:ss.ffffffZ}'";
     
        var dateTo = $"'{DateTime.Parse(dateToString):yyyy-MM-ddTHH:mm:ss.ffffffZ}'";

        var sql = $"""
                SELECT 
                timestamp,
                ask,
                bid
                FROM '{symbol}'
                WHERE timestamp >= {dateFrom} 
                AND timestamp <= {dateTo}
                ORDER BY timestamp ASC
                LIMIT {size}
            """;

        await using var command = dataSource.CreateCommand(sql);
        using var reader = await command.ExecuteReaderAsync();

        int count = 0;
        DateTime lastDate = DateTime.MaxValue;

        while (await reader.ReadAsync())
        {
            count++; // to track results
            if (count % 5 != 0)
            {
                continue; // skip every 10th tick
            }
            lastDate = reader.GetDateTime(0);
            await ProcessTick(new PriceObj
            {
                symbol = symbol,
                date = lastDate,
                ask = Convert.ToDecimal(reader.GetDouble(1)),
                bid = Convert.ToDecimal(reader.GetDouble(2))
            });
        }

        return (count == size, lastDate);
    }

    private async Task ProcessTick(PriceObj priceObj)
    {

        // Invoke all the strategies defined in configuration
        // await strategies.All(async x=> await x.Invoke(priceObj));

        await strategy.Invoke(priceObj, Program.BACKTESTING_STRATEGY_DEFINITION!);

        // Review open positions, check if the new symbol data meets the threshold for LIMI/STOP levels
        await this.positions.Review(priceObj);
        // this.positions.TrailingStopLoss(priceObj);
        this.positions.ReviewEquity(priceObj);
    }

    public static async Task<List<OhlcObject>> GetOHLCData(string symbol, int OHLC_MINUTES, int OHLC_COUNT)
    {
        var dateFrom = $"dateadd('M', -{last_months}, now())";
        var days = ((OHLC_MINUTES * OHLC_COUNT) + 1439) / 1440 + 10;
        var limit = (OHLC_COUNT * 3) / 2;

        var sql = $"""
            SELECT
                timestamp AS date,
                first(ask) AS open,
                max(ask) AS high,
                min(ask) AS low,
                last(ask) AS close,
                count() as ticks
            FROM '{symbol}'
            WHERE timestamp >= {dateFrom} 
            SAMPLE BY {OHLC_MINUTES}m ALIGN TO CALENDAR
            ORDER BY date ASC
            LIMIT {limit}
            """;

        try
        {

            await using var command = dataSource.CreateCommand(sql);
            using var reader = await command.ExecuteReaderAsync();

            var result = new List<OhlcObject>();

            while (await reader.ReadAsync())
            {
                var ticks = reader.GetInt32(5);
                var date = reader.GetDateTime(0);
                if (ticks >= 10)
                {
                    result.Add(new OhlcObject
                    {
                        date = date,
                        open = Convert.ToDecimal(reader.GetDouble(1)),
                        high = Convert.ToDecimal(reader.GetDouble(2)),
                        low = Convert.ToDecimal(reader.GetDouble(3)),
                        close = Convert.ToDecimal(reader.GetDouble(4)),
                    });
                }
            }

            return result.OrderBy(x => x.date).TakeLast(OHLC_COUNT).ToList(); ;
        }
        catch (Exception ex)
        {
            throw new TradingException($"Error retrieving OHLC data: {ex.Message}", ex);
        }

    }
}