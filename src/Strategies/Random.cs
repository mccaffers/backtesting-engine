using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
    readonly IRequestOpenTrade requestOpenTrade;
    readonly IEnvironmentVariables envVariables;

    public RandomStrategy(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.envVariables = envVariables;
    }

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public void Invoke(PriceObj priceObj)
    {

        var randomInt = new Random().Next(2); 
        // 0 or 1
        TradeDirection direction = TradeDirection.BUY;
        if (randomInt== 0)
        { 
            direction = TradeDirection.SELL;
        }

        var openOrderRequest = new RequestObject(priceObj)
        {
            direction = direction,
            size = decimal.Parse(envVariables.tradingSize),
            stopDistancePips = decimal.Parse(envVariables.stopDistanceInPips),
            limitDistancePips = decimal.Parse(envVariables.limitDistanceInPips),
        };

        this.requestOpenTrade.Request(openOrderRequest);
    }
}
