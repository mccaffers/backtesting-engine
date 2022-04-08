using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;

namespace backtesting_engine;

public class TradingObjects : ITradingObjects
{
    public TradingObjects() {
        accountObj = new AccountObj(openTrades, tradeHistory);
    }

    public ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject>();
    public string test { get; set; } = "";
    public DateTime tradeTime { get; set; }
    public AccountObj accountObj {get; init;}

}