using backtesting_engine.interfaces;
using backtesting_engine_models;

namespace backtesting_engine;

public class OpenOrder : TradingBase, IOpenOrder
{

    public OpenOrder(IServiceProvider provider) : base(provider) { 
        
    }

    // Data Update
    public void Request(RequestObject reqObj)
    {
        tradingObjects.openTrades.TryAdd(reqObj.dealReference, reqObj);
    }
}
