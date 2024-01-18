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

        // Moved trading restrictions (eg. max open trades) into the strategy
        tradingObjects.openTrades.TryAdd(reqObj.key, reqObj);
        webNotification.OpenTrades(reqObj);
    }
}
