using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_operations;

public class BacktestingOpenTrades : IOpenTrades
{

    protected readonly ITradingObjects tradeObjs;

    public BacktestingOpenTrades(ITradingObjects tradeObjs)
    {
        this.tradeObjs = tradeObjs;
    }

    public Task<int> Request(string symbol)
    {
        return Task.FromResult(tradeObjs.openTrades.Count);
    }
}