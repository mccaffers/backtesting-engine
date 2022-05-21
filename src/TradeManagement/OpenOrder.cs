using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine;

public interface IOpenOrder
{
    void Request(RequestObject reqObj);
}

public class OpenOrder : TradingBase, IOpenOrder
{
    public OpenOrder(IServiceProvider provider) : base(provider) { }

    // Data Update
    public void Request(RequestObject reqObj)
    {

        
        // One trade open at the moment
        var openTradesCount = this.tradingObjects.openTrades.Count(x => x.Key.Contains(reqObj.priceObj.symbol));
        if (openTradesCount != 0)
        {
            return;
        }

        this.tradingObjects.openTrades.TryAdd(reqObj.key, reqObj);
    }
}
