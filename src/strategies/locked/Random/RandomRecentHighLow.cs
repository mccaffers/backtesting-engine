using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class RandomRecentHighLow : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public RandomRecentHighLow(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder, webNotification) { }

    private OhlcObject lastItem = new OhlcObject();

    private DateTime lastTraded = DateTime.MinValue;

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(30), ohlcList);

        // Maximum of one trade open at a time
        // Conditional to only invoke the strategy if there are no trades open
        if (tradeObjs.openTrades.Count() >= 3)
        {
            return;
        }
        
        if(priceObj.date.Subtract(lastTraded).TotalHours < 8){
            return;
        }

        
        // Keep 5 hours of history (30*10)
        // Only start trading once you have 5 hours
        if (ohlcList.Count > envVariables.randomStrategyAmountOfHHLL)
        {

            lastTraded=priceObj.date;

            // Get the highest value from all of the OHLC objects
            var recentHigh = ohlcList.Max(x => x.high);

            // Get the lowest value from all of the OHLC objects
            var recentLow = ohlcList.Min(x => x.low);

            var randomInt = new Random().Next(2); // 0 or 1

            // Default to BUY, randomly switch
            TradeDirection direction = TradeDirection.BUY;
            if (randomInt == 0)
            {
                direction = TradeDirection.SELL;
            }

            var stopLevel = direction == TradeDirection.BUY ? recentLow : recentHigh;
            var limitLevel = direction == TradeDirection.BUY ? ohlcList.TakeLast(2).Max(x => x.high) : 
                                                                ohlcList.TakeLast(2).Min(x => x.low);

            var stopDistance = direction == TradeDirection.SELL ? (stopLevel - priceObj.bid) : (priceObj.ask - stopLevel);
            stopDistance = stopDistance * envVariables.GetScalingFactor(priceObj.symbol);

            var limitDistance = direction == TradeDirection.BUY ? (limitLevel - priceObj.bid) : (priceObj.ask - limitLevel);
            limitDistance = limitDistance * envVariables.GetScalingFactor(priceObj.symbol);

            // Ensure there is enough distance and isn't going to be immediately auto stopped out
            if (stopDistance < 20 || limitDistance < 20)
            {
                return;
            }

            // Stop any excessive stop losses 
            if(stopDistance > 100){
                stopDistance = 100;
            }

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
