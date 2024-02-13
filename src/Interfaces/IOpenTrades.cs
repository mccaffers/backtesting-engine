using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface IOpenTrades
{
    Task<int> Request(string symbol);
}