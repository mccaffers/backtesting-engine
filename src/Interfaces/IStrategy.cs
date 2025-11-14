using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface IStrategy
{

    Task Invoke(PriceObj priceObj, StrategyDefinition strategy);
    
    public Task During(PriceObj priceObj) {
        throw new ArgumentException("Exit not implemented");
    }
    public Task Exit() {
        return Task.CompletedTask;
    }

}