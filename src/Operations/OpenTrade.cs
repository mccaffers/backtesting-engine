using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Utilities;

public static class OpenTrade
{
    public static void Request(PriceObj priceObj, ConcurrentDictionary<string, OpenTradeObject> openTrades, RequestObject openOrderRequest){   

        // TODO
        // One trade open at the moment
        var openTradesCount = openTrades.Where(x => x.Key.Contains(priceObj.symbol)).Count();
        if(openTradesCount!=0){
            return;
        }
        
        System.Console.WriteLine("Opened trade for " + priceObj.symbol);
      
        openTrades.TryAdd(DictoinaryKeyStrings.OpenTrade(priceObj), RequestOpenTrade.Request(openOrderRequest));
    }
}