using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;

public interface ITradingObjects
{
    ConcurrentDictionary<string, RequestObject> openTrades { get; }
    ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; }
    string test { get; set; }
    DateTime tradeTime { get; set; }
}

public class TradingObjects : ITradingObjects
{
    public ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject>();
    public string test { get; set; } = "";
    public DateTime tradeTime { get; set; }
    public static AccountObj accountObj;

}