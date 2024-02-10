

using backtesting_engine;
using backtesting_engine.interfaces;
using Utilities;

namespace backtesting_engine;

public class StrategyObjects : IStrategyObjects
{
    private DateTime lastTraded = DateTime.MinValue;

    public async Task<List<OhlcObject>> GetOHLCObject(PriceObj priceObj, decimal price, TimeSpan duration, List<OhlcObject> OHLCArray)
    {
        return await Task.FromResult(GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, duration, OHLCArray));
    }

    public async Task<DateTime> GetLastTraded(PriceObj priceObj)
    {
         return await Task.FromResult(lastTraded);
    }

    public async Task SetLastTraded(PriceObj priceObj)
    {
        lastTraded=priceObj.date;
        await Task.CompletedTask;
    }
}