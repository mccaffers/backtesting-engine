using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class RandomRecentHighLow_DRAFT : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public RandomRecentHighLow_DRAFT(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder, webNotification) { }

    private OhlcObject lastItem = new OhlcObject();

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

        // Maximum of one trade open at a time
        // Conditional to only invoke the strategy if there are no trades open
        if (tradeObjs.openTrades.Count() >= 1)
        {
            return;
        }

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(30), ohlcList);

        if (ohlcList.Count > 1)
        {
            var secondLast = ohlcList[ohlcList.Count - 2];
            if (secondLast.complete && secondLast.close != lastItem.close)
            {
                await webNotification.PriceUpdate(secondLast, true);
                lastItem = secondLast;
            }
            else
            {
                await webNotification.PriceUpdate(ohlcList.Last());
            }
        }

        // Keep 5 hours of history (30*10)
        if (ohlcList.Count > 10)
        {

            // Get the highest value from all of the OHLC objects
            var recentHigh = ohlcList.Max(x => x.high);
            
            // Get the lowest value from all of the OHLC objects
            var recentLow = ohlcList.Min(x => x.low);

            var randomInt = new Random().Next(2); // 0 or 1

            // Default to BUY, randomly switch
            TradeDirection direction = TradeDirection.SELL;
            if (randomInt == 0)
            { 
                direction = TradeDirection.SELL;
            } 


            var highDiff = recentHigh - priceObj.ask;
            var lowerDIff = priceObj.ask - recentLow;

            if (highDiff < lowerDIff)
            {
                direction = TradeDirection.BUY;
            }

            var stopDistance = 10m;
            var limitLevel = 0m;

            if (direction == TradeDirection.BUY)
            {
                // stopLevel = recentLow;
                limitLevel = recentHigh;
                stopDistance = 10;
            }
            else
            {
                // stopLevel = recentHigh;
                limitLevel = recentLow;
                stopDistance = 10;
            }

            var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

            var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
            {
                size = decimal.Parse(envVariables.tradingSize),
                stopDistancePips = stopDistance,
                limitLevel = limitLevel

            };

            this.requestOpenTrade.Request(openOrderRequest);

            ohlcList.RemoveAt(0);
        }
        // return true;
    }
}
