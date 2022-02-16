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

        var openOrderRequest = new RequestObject(){
            level = priceObj.bid,
            direction = TradeDirection.BUY,
            scalingFactor = priceObj.scalingFactor,
            size = 1
        };

        OpenTrade.Request(priceObj, Program.openTrades, openOrderRequest);
            
    }
}
