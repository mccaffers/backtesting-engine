using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;


public class RandomWithCloseAtHhll : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public RandomWithCloseAtHhll(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder,  webNotification) {}

    private OhlcObject lastItem = new OhlcObject();

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(30), ohlcList);

        if(ohlcList.Count>1){
            var secondLast = ohlcList[ohlcList.Count - 2];
            if(secondLast.complete && secondLast.close!=lastItem.close){
                await webNotification.PriceUpdate(secondLast, true);
                lastItem=secondLast;
            } else {
                await webNotification.PriceUpdate(ohlcList.Last());
            }
        }

        // Keep 30 days of history
        if(ohlcList.Count > 10){
            
            var recentHigh = ohlcList.Max(x=>x.high);
            var recentLow = ohlcList.Min(x=>x.low);

            var randomInt = new Random().Next(2); // 0 or 1

            // Default to BUY
            TradeDirection direction = TradeDirection.SELL;
            // if (randomInt== 0)
            // { 
            //     direction = TradeDirection.SELL;
            // } 

            var highDiff = recentHigh - priceObj.ask;
            var lowerDIff = priceObj.ask - recentLow;

            if(highDiff < lowerDIff){
                direction = TradeDirection.BUY;
            }

            var stopDistance = 10m;
            var limitLevel = 0m;

            if(direction == TradeDirection.BUY){
                // stopLevel = recentLow;
                limitLevel = recentHigh;
                stopDistance=10;
            } else {
                // stopLevel = recentHigh;
                limitLevel = recentLow;
                stopDistance=10;
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
