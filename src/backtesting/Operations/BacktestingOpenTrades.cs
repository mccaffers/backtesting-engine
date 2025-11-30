using backtesting_engine.interfaces;

namespace backtesting_engine_operations;

public class BacktestingOpenTrades : IOpenTrades
{

    protected readonly ITradingObjects tradeObjs;

    public BacktestingOpenTrades(ITradingObjects tradeObjs)
    {
        this.tradeObjs = tradeObjs;
    }

    public Task<int> Request(string symbol, string strategyId)
    {
        return Task.FromResult(tradeObjs.openTrades.Count);
    }
}