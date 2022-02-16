using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest;

public class RandomStrategy
{
    public static void Invoke(PriceObj priceObj){


        int rnd = new Random().Next(2);
        TradeDirection direction;
        if(rnd == 0){
            direction = TradeDirection.BUY;
        } else {
            direction = TradeDirection.SELL;
        }

        var openOrderRequest = new RequestObject(priceObj){
            direction = direction,
            scalingFactor = priceObj.scalingFactor,
            size = 1,
            stopLevel = 50/priceObj.scalingFactor,
            limitLevel= 50/priceObj.scalingFactor
        };

        OpenTrade.Request(priceObj, Program.openTrades, openOrderRequest);
            
    }
}
