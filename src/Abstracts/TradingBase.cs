using backtesting_engine.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace backtesting_engine;

public abstract class TradingBase {

    protected ITradingObjects tradingObjects;
    protected ISystemObjects systemObjects;

    protected TradingBase(IServiceProvider provider)
    {
        this.tradingObjects = provider.GetRequiredService<ITradingObjects>();
        this.systemObjects = provider.GetRequiredService<ISystemObjects>();

    }
 }