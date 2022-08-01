namespace backtesting_engine.interfaces;

public interface IStrategy
{
    Task Invoke(PriceObj priceObj);
}