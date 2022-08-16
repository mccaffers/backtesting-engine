using backtesting_engine.interfaces;
using backtesting_engine_models;

namespace backtesting_engine;


// To be injected when backtesting without the web viewer (headless)
public class EmptyWebNotification : IWebNotification
{
    public EmptyWebNotification(){
    }

    public Task AccountUpdate(decimal input)
    {
        return Task.CompletedTask;
    }

    public Task PriceUpdate(OhlcObject input, bool force)
    {
        return Task.CompletedTask;
    }

    public Task TradeUpdate(TradeHistoryObject input)
    {
        return Task.CompletedTask;
    }

}