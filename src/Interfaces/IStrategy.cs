namespace backtesting_engine.interfaces;

public interface IStrategy
{
    bool Invoke(PriceObj priceObj);
}