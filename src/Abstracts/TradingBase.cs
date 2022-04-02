using Microsoft.Extensions.DependencyInjection;

namespace backtesting_engine;

public abstract class TradingBase {

    public string test = "";
    protected ITradingObjects tradingObjects;

    public TradingBase(IServiceProvider provider)
    {
        this.tradingObjects = provider.GetRequiredService<ITradingObjects>();
    }
 }