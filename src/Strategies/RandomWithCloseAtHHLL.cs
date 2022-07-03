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


public class RandomWithCloseAtHhll : IStrategy
{
    readonly IRequestOpenTrade requestOpenTrade;
    readonly IEnvironmentVariables envVariables;
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public RandomWithCloseAtHhll(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.envVariables = envVariables;
    }

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public void Invoke(PriceObj priceObj)
    {

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromHours(envVariables.randomStrategyAmountOfHHLL), ohlcList);

        // Keep 30 days of history
        if(ohlcList.Count > 10){
            
            var recentHigh = ohlcList.Max(x=>x.high);
            var recentLow = ohlcList.Min(x=>x.low);

            var randomInt = new Random().Next(2); // 0 or 1

            // Default to BUY
            TradeDirection direction = TradeDirection.BUY;
            if (randomInt== 0)
            { 
                direction = TradeDirection.SELL;
            } 

            var stopLevel = 0m;
            var limitLevel = 0m;

            if(direction == TradeDirection.BUY){
                stopLevel = recentLow;
                limitLevel = recentHigh;
            } else {
                stopLevel = recentHigh;
                limitLevel = recentLow;
            }

            var openOrderRequest = new RequestObject(priceObj, direction, envVariables)
            {
                size = decimal.Parse(envVariables.tradingSize),
                stopLevel = stopLevel,
                limitLevel = limitLevel
               
            };

            this.requestOpenTrade.Request(openOrderRequest);

            ohlcList.RemoveAt(0);
        }
    }
}
