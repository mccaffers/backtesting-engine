using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class RandomRecentHighLow_COPY : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public RandomRecentHighLow_COPY(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder, webNotification) { }

    private OhlcObject lastItem = new OhlcObject();

    private DateTime lastTraded = DateTime.MinValue;

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(30), ohlcList);

        // Maximum of one trade open at a time
        // Conditional to only invoke the strategy if there are no trades open
        if (tradeObjs.openTrades.Count() >= 1)
        {
            return;
        }
        
        if(priceObj.date.Subtract(lastTraded).TotalHours < 6){
            return;
        }

        
        // Keep 5 hours of history (30*10)
        // Only start trading once you have 5 hours
        if (ohlcList.Count > envVariables.randomStrategyAmountOfHHLL)
        {

            lastTraded=priceObj.date;

            // Get the highest value from all of the OHLC objects
            var recentHigh = ohlcList.Max(x => x.low);

            // Get the lowest value from all of the OHLC objects
            var recentLow = ohlcList.Min(x => x.close);

            var randomInt = new Random().Next(2); // 0 or 1

            // Default to BUY, randomly switch
            TradeDirection direction = TradeDirection.BUY;
            if (randomInt == 0)
            {
                direction = TradeDirection.SELL;
            }

            var stopLevel = direction == TradeDirection.BUY ? recentLow : recentHigh;
            var limitLevel = direction == TradeDirection.BUY ? recentHigh : recentLow;

            var stopDistance = direction == TradeDirection.SELL ? (stopLevel - priceObj.bid) : (priceObj.ask - stopLevel);
            stopDistance = stopDistance * envVariables.GetScalingFactor(priceObj.symbol);

            var limitDistance = direction == TradeDirection.BUY ? (limitLevel - priceObj.bid) : (priceObj.ask - limitLevel);
            limitDistance = limitDistance * envVariables.GetScalingFactor(priceObj.symbol);

            // System.Console.WriteLine("limit: " + limitDistance + " stop: "+ stopDistance);
            // System.Console.WriteLine(limitDistance-stopDistance);
            // if(stopDistance < limitDistance){
            //     var swapSL = stopDistance;
            //     stopDistance = limitDistance;
            //     limitDistance = swapSL;
            //     direction = direction == TradeDirection.BUY ? TradeDirection.SELL : TradeDirection.BUY;
            // }
            // Ensure there is enough distance and isn't going to be immediately auto stopped out
            if (stopDistance < 10 || limitDistance < 10)
            {
                return;
            }

            // Stop any excessively large stop losses
            if(stopDistance > 50){
                // stopDistance = 50;
            }

            // Stop any crazy amounts
            if(limitDistance > 50){
                // limitDistance = 50;
            }

            // System.Console.WriteLine(stopLevel);
            var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

            var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
            {
                size = decimal.Parse(envVariables.tradingSize),
                stopDistancePips = stopDistance,
                limitDistancePips = limitDistance

            };

            this.requestOpenTrade.Request(openOrderRequest);

            ohlcList.RemoveAt(0);
        }
    }
}
