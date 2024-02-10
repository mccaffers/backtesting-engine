

using backtesting_engine;
using backtesting_engine.interfaces;
using Utilities;

namespace backtesting_engine;

public class StrategyObjects : IStrategyObjects
{
    public List<OhlcObject> GetOHLCObject(PriceObj priceObj, decimal price, TimeSpan duration, List<OhlcObject> OHLCArray)
    {
        return GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, duration, OHLCArray);
    }
}