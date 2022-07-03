namespace backtesting_engine.interfaces;

public interface IStrategy
{
    void Invoke(PriceObj priceObj);
}