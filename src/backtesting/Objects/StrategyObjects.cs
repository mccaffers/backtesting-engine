

using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine;

public class StrategyObjects : IStrategyObjects
{
    private DateTime lastTraded = DateTime.MinValue;

    public async Task<DateTime> GetLastTraded(PriceObj priceObj, StrategyDefinition strategy)
    {
         return await Task.FromResult(lastTraded);
    }

    public async Task SetLastTraded(PriceObj priceObj, StrategyDefinition strategy)
    {
        lastTraded=priceObj.date;
        await Task.CompletedTask;
    }

    public async Task<List<OhlcObject>> GetOHLCObject(PriceObj priceObj, decimal price, int count, int minutes, List<OhlcObject> OHLCArray)
    {
        return await Task.FromResult(GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(minutes), OHLCArray));
    }
}