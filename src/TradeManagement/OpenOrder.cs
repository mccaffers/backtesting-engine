using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine;

public static class OpenOrder
{
    // Data Update
    public static void Request(RequestObject reqObj){   

        // One trade open at the moment
        var openTradesCount = Program.openTrades.Count(x => x.Key.Contains(reqObj.priceObj.symbol));
        if(openTradesCount!=0){
            return;
        }

        Program.openTrades.TryAdd(reqObj.key, reqObj);
    }
}
