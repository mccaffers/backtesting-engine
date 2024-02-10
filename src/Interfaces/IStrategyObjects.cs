namespace backtesting_engine.interfaces;

public interface IStrategyObjects
{
    Task<List<OhlcObject>> GetOHLCObject(PriceObj priceObj, decimal price, TimeSpan duration, List<OhlcObject> OHLCArray);
    Task<DateTime> GetLastTraded(PriceObj priceObj);
    Task SetLastTraded(PriceObj priceObj);
}