using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;

public static class OpenTrade
{
    public static void Request(PriceObj priceObj, ConcurrentDictionary<string, OpenTradeObject> openTrades){   

        // TODO
        // One trade open at the moment
        var openTradesCount = openTrades.Select(x => x.Key.Contains(priceObj.symbol)).Count();
        if(openTradesCount!=0){
            return;
        }
        
        System.Console.WriteLine("Opened trade for " + priceObj.symbol);
        openTrades.TryAdd(priceObj.symbol, RequestOpenTrade.Request(new RequestObject(){
            level = priceObj.ask,
            direction = "BUY",
            scalingFactor = priceObj.scalingFactor,
            size = 1
        }));
    }
}