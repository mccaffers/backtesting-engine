namespace backtesting_engine.interfaces;

public interface IStrategyObjects
{
    List<OhlcObject> GetOHLCObject(PriceObj priceObj, decimal price, TimeSpan duration, List<OhlcObject> OHLCArray);
}