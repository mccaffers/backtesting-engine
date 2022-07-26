using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine;

public class TradingObjects : ITradingObjects
{
    public TradingObjects(IEnvironmentVariables envVariables) {
        accountObj =  new AccountObj(openTrades, 
                                    tradeHistory,
                                    decimal.Parse(envVariables.accountEquity),
                                    decimal.Parse(envVariables.maximumDrawndownPercentage),
                                    envVariables);
    }

    public ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject>();
    public string test { get; set; } = "";
    public DateTime tradeTime { get; set; }
    public AccountObj accountObj {get; init;}
    // public interfaces.IAccountObj accountObj { get; init; }
}