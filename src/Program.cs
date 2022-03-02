using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine;

public static class Program
{

    public readonly static ConcurrentDictionary<string, RequestObject> openTrades = new ConcurrentDictionary<string, RequestObject>();
    public readonly static ConcurrentDictionary<string, TradeHistoryObject> tradeHistory = new ConcurrentDictionary<string, TradeHistoryObject >();

    public readonly static AccountObj accountObj = new AccountObj();
    public readonly static EnvironmentVariables env = new EnvironmentVariables();

    // Dependency Injection Scope
    public static IServiceScope scope;

    public static async Task Main(string[] args)
    {
        Program.accountObj.openingEquity = decimal.Parse(env.Get("accountEquity"));
        Program.accountObj.maximumDrawndownPercentage = decimal.Parse(env.Get("maximumDrawndownPercentage"));

        scope = RegisterServices().CreateScope(); // Save scope to resovle dependencies

        await new Main().IngestAndConsume(new Consumer(), new Ingest());
    }
}
