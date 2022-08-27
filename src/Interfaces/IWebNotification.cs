using backtesting_engine;
using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface IWebNotification
{
    Task PriceUpdate(OhlcObject input, bool force = false);
    Task AccountUpdate(decimal input);
    Task OpenTrades(List<KeyValuePair<string, RequestObject>> input);
    Task TradeUpdate(TradeHistoryObject input);
}

