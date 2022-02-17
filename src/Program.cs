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

    public static ConcurrentDictionary<string, RequestObject> openTrades = new ConcurrentDictionary<string, RequestObject>();
    public static ConcurrentDictionary<string, TradeHistoryObject> tradeHistory = new ConcurrentDictionary<string, TradeHistoryObject >();

    public static async Task Main(string[] args)
    {
        await new Main().IngestAndConsume(new Consumer(), new Ingest());
    }
}
