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


        TradeDirection direction = TradeDirection.BUY;
        if(new Random().Next(2) == 0){ // 0 or 1
            direction = TradeDirection.SELL;
        }

        var openOrderRequest = new RequestObject(priceObj){
            direction= direction,
            size=1,
            stopDistancePips=50,
            limitDistancePips=50,
        };

        RequestOpenTrade.Request(openOrderRequest);

            
    }
}
