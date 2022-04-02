using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;

public interface ITradingObjects
{
    ConcurrentDictionary<string, RequestObject> openTrades { get; }
    ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; }
    string test { get; set; }
    DateTime tradeTime { get; set; }
    AccountObj accountObj {get; init;}
}

public class TradingObjects : ITradingObjects
{
    public TradingObjects(IServiceProvider provider) {
        accountObj = new AccountObj(openTrades, tradeHistory);
    }

    public ConcurrentDictionary<string, RequestObject> openTrades { get; } = new ConcurrentDictionary<string, RequestObject>();
    public ConcurrentDictionary<string, TradeHistoryObject> tradeHistory { get; } = new ConcurrentDictionary<string, TradeHistoryObject>();
    public string test { get; set; } = "";
    public DateTime tradeTime { get; set; }
    public AccountObj accountObj {get; init;}

}