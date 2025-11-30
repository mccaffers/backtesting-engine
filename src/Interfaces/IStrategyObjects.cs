using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface IStrategyObjects
{
    // Task<List<OhlcObject>> GetOHLCObject(PriceObj priceObj, decimal price, TimeSpan duration, List<OhlcObject> OHLCArray);
    Task<List<OhlcObject>> GetOHLCObject(PriceObj priceObj, decimal price,  int count, int minutes, List<OhlcObject> OHLCArray);
    Task<DateTime> GetLastTraded(PriceObj priceObj, StrategyDefinition strategy);
    Task SetLastTraded(PriceObj priceObj, StrategyDefinition strategy);
}