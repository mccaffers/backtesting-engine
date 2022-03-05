using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;

public interface IStrategy
{
    void Invoke(PriceObj priceObj);
}

public class RandomStrategy : IStrategy
{
    public void Invoke(PriceObj priceObj)
    {

        TradeDirection direction = TradeDirection.BUY;
        #pragma warning disable S2245 // Weak Cryptography Warning
        var randomInt = new Random().Next(2); // No need for slow crypto function, output is used to determine BUY OR SELL only
        #pragma warning restore S2245

        if (randomInt== 0)
        { // 0 or 1
            direction = TradeDirection.SELL;
        }

        var openOrderRequest = new RequestObject(priceObj)
        {
            direction = direction,
            size = 1,
            stopDistancePips = 50,
            limitDistancePips = 50,
        };

        RequestOpenTrade.Request(openOrderRequest);


    }
}
