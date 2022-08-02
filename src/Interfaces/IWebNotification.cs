using backtesting_engine;
using backtesting_engine_models;

public interface IWebNotification
{
    Task Message(OhlcObject input, bool force = false);
    Task AccountUpdate(decimal input);
    Task TradeUpdate(TradeHistoryObject input);
}

