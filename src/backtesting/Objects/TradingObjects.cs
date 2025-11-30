using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine;

public class TradingObjects : ITradingObjects
{
    public TradingObjects() {
        accountObj =  new AccountObj(openTrades, 
                                    tradeHistory,
                                    decimal.Parse(BACKTESTING.ACCOUNT_EQUITY.Value()),
                                    decimal.Parse(BACKTESTING.MAXIMUM_DRAWNDOWN_PERCENTAGE.Value()));
    }

    public Dictionary<string, RequestObject> openTrades { get; } = new Dictionary<string, RequestObject>();
    public Dictionary<string, TradeHistoryObject> tradeHistory { get; } = new Dictionary<string, TradeHistoryObject>();
    public PriceObj? lastPrice { get; set; } = null;
    public AccountObj accountObj {get; init;}
}