using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine;

public class OpenOrder : TradingBase, IOpenOrder
{
    readonly IWebNotification webNotification;

    public OpenOrder(IServiceProvider provider, IWebNotification webNotification) : base(provider) { 
        this.webNotification = webNotification;
    }

    // Data Update
    public void Request(RequestObject reqObj)
    {

        // One trade open at the moment
        var openTradesCount = this.tradingObjects.openTrades.Count(x => x.Key.Contains(reqObj.priceObj.symbol));
        
        // Maximum of one open trade
        // TODO Make configurable
        if (openTradesCount < 1)
        {
            // System.Console.WriteLine(reqObj.openDate + " " + reqObj.direction + " " + reqObj.level);
            this.tradingObjects.openTrades.TryAdd(reqObj.key, reqObj);
            webNotification.OpenTrades(reqObj);
        }

    }
}
