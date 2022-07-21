using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

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
    public bool Invoke(PriceObj priceObj)
    {

        if(priceObj.date.DayOfWeek == DayOfWeek.Sunday)
        {
            return true;
        }

        if(priceObj.date.DayOfWeek == DayOfWeek.Friday && priceObj.date.Hour > 14){
            return true;
        }

        if(priceObj.date.Hour < 5 || priceObj.date.Hour > 19){
            return true;
        }

        var randomInt = new Random().Next(2); 
        // 0 or 1
        TradeDirection direction = TradeDirection.BUY;
        if (randomInt== 0)
        { 
            direction = TradeDirection.SELL;
        }

        var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
        {
            size = decimal.Parse(envVariables.tradingSize),
            stopDistancePips = decimal.Parse(envVariables.stopDistanceInPips),
            limitDistancePips = decimal.Parse(envVariables.limitDistanceInPips),
        };

        this.requestOpenTrade.Request(openOrderRequest);
        return true;
    }
}
