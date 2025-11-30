using System.Collections.Concurrent;
using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface ITradingObjects
{
    Dictionary<string, RequestObject> openTrades { get; }
    Dictionary<string, TradeHistoryObject> tradeHistory { get; }
    PriceObj? lastPrice { get; set; }
    AccountObj accountObj {get; init;}
}