using System.Collections.Concurrent;
using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface ITradingObjects
{
    ConcurrentDictionary<string, RequestObject> openTrades { get; }
    ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; }
    string test { get; set; }
    DateTime tradeTime { get; set; }
    AccountObj accountObj {get; init;}
}