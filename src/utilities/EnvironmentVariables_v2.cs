using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json;

namespace Utilities;

public enum LoggingVariables
{
    SYSTEM_LOG,
    CONSOLE_LOG,
    LAMBDA_LOG,
    REPORT_TO_ELASTICSEARCH
}

public enum SYSTEM
{
    ENVIRONMENT
}

public enum ELASTICSEARCH
{
    ELASTIC_USER,
    ELASTIC_PASSWORD,
    CLOUD_ID
}

public enum BACKTESTING
{
    RUN_ID,
    RUN_ITERATION,
    FASTER_PROCESSING_BY_SKIPPING_SOME_DATA,
    SYMBOLS,
    SYMBOL_FOLDER,
    TICK_DATA_FOLDER,
    LAST_MONTHS,
    DATE_TO,
    PULL_FROM_AWS_S3,
    CLEAN_LOCAL_TICK_FOLDER,
    INSTANCE_COUNT,
    REPORT_INDIVIDUAL_TRADES,
    REPORT_LOSSES,
    DATA_HOST_URL,
    S3_PATH,
    S3_BUCKET,
    ACCOUNT_EQUITY,
    MAXIMUM_DRAWNDOWN_PERCENTAGE,
    MAX_MONTHLY_DRAWDOWN_THRESHOLD,
    MAX_OVERALL_DRAWDOWN_THRESHOLD,
    MIN_TRADE_RATIO
}

public enum TradingVariables
{
    STRATEGY,
    SCALING_FACRTOR,
    STOP_DISTANCE_IN_PIPS,
    LIMIT_DISTANCE_IN_PIPS,
    TRAILING_STOP_LOSS_ACTIVE,
    TRAILING_STOP_LOSS_VALUE,
    MOVING_LIMIT_VALUE,
    TRADING_SIZE,
    STRATEGY_ID
}

public enum StrategyVariables
{
    VARIABLE_A,
    VARIABLE_B,
    VARIABLE_C,
    VARIABLE_D
}

public static class EnumExtensions
{

    public static string Value<T>(this T value) where T : Enum
    {
        return EnvironmentVariables.Get(value);
    }

}

public class IgnoreEnvironmentVariables
{
    public static string[] List = {
        "a"
    };
}

public class EnvironmentVariables
{


    // public static IEnumerable<DictionaryEntry>? entries = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>();
    // public static Dictionary<Enum, String> VariableInjectList = [];
    public static readonly ConcurrentDictionary<Enum, string> VariableInjectList = new ConcurrentDictionary<Enum, string>();

    public static void Inject(Enum key, string value)
    {
        VariableInjectList.AddOrUpdate(key, value, (k, v) => value);
    }
    
    private static IEnumerable<DictionaryEntry>? entries = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>();
    private static Dictionary<String, String> envCache = [];

    static EnvironmentVariables(){
        if (envCache.Count == 0)
        {
            if (entries != null)
            {
                foreach (var item in entries)
                {
                    var key = item.Key?.ToString();
                    var entryValue = item.Value?.ToString();
                    if (key != null && entryValue != null && !ReportEnvironmentVariables.ignoreList.Contains(key))
                    {
                        envCache.Add(key, entryValue);
                    }
                }
            }
        }
    }

    // public static void ReloadEnvironmentVariables()
    // {
    //     entries = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>();
    //     envCache = new();
    //     VariableInjectList.Clear();
    // }

    public static string Get(Enum value)
    {

        if (VariableInjectList.Count > 0)
        {
            string? output;
            if (VariableInjectList.TryGetValue(value, out output))
            {
                return output;
            }
        }

        return envCache[value.ToString()];

    }

    public static string Hostname()
    {
        return Dns.GetHostName();
    }

    public static Dictionary<string, decimal> GetScalingFactorDictionary()
    {
        var localdictionary = new Dictionary<string, decimal>();
        foreach (var symbol in TradingVariables.SCALING_FACRTOR.Value().Split(";"))
        {
            if (string.IsNullOrEmpty(symbol))
            {
                continue;
            }

            var scalingFactorArray = symbol.ToString().Split(",");
            decimal sf;
            if (!decimal.TryParse(scalingFactorArray[1], out sf))
            {
                throw new ArgumentException("Cannot read scaling factor of symbol");
            }
            localdictionary.Add(scalingFactorArray[0], sf);
        }
        return localdictionary;
    }

    public static decimal GetScalingFactor(string symbol)
    {
        var output = 0m;

        // Scaling Factor is missing
        if (GetScalingFactorDictionary().TryGetValue(symbol, out output) == false)
        {
            throw new ArgumentException(symbol + " is missing scaling factor");
        }
        return output;
    }
}