using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Utilities;

namespace backtesting_engine;

public static class OpenTrade
{
    // Data Update
    public static void Request(RequestObject reqObj){   

        // TODO
        // One trade open at the moment
        var openTradesCount = Program.openTrades.Where(x => x.Key.Contains(reqObj.priceObj.symbol)).Count();
        if(openTradesCount!=0){
            return;
        }
        
        // System.Console.WriteLine("Opened trade for " + reqObj.priceObj.symbol + " " + reqObj.direction);
      
        Program.openTrades.TryAdd(reqObj.key, reqObj);
    }
}
