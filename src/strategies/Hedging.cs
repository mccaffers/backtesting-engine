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

public class HedgeStrategy : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();
    private OhlcObject lastItem = new OhlcObject();

    public HedgeStrategy(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder,  webNotification) {}

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public Task Invoke(PriceObj priceObj)
    {
        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(30), ohlcList);

        if(ohlcList.Count > 10){
            ohlcList.RemoveAt(0);
        }
        if(priceObj.date.DayOfWeek == DayOfWeek.Sunday)
        {
            return Task.CompletedTask;
        }

        if(priceObj.date.DayOfWeek == DayOfWeek.Friday && priceObj.date.Hour > 14){
            return Task.CompletedTask;
        }

        if(priceObj.date.Hour < 5 || priceObj.date.Hour > 19) {
            return Task.CompletedTask;
        }

        var randomInt = new Random().Next(2); 
        // 0 or 1
        TradeDirection direction = TradeDirection.BUY;
        if (randomInt== 0)
        { 
            direction = TradeDirection.SELL;
        }

        var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        
        var plannedTradingSize = decimal.Parse(envVariables.tradingSize);
        var plannedStopLoss = decimal.Parse(envVariables.stopDistanceInPips);
        var plannedLimit = decimal.Parse(envVariables.limitDistanceInPips);

        foreach(var item in this.tradeObjs.openTrades.Where(x => x.Key.Contains(priceObj.symbol)).Select(x => x.Value)){
            if(priceObj.date.Subtract(item.openDate).TotalHours > 1){
                this.closeOrder.Request(item, priceObj);
                return Task.CompletedTask;
            }
        }

        var openTradeBool = true;
        if(tradeObjs.openTrades.Count > 0){

            // There are negative trades, lets get the one that is most negative
            // if(tradeObjs.openTrades.Any(x=>x.Value.profit < 0)){
            //     var mostNegative = tradeObjs.openTrades.OrderBy(x=>x.Value.profit).First();
            //     // if(mostNegative.Value.profit < -5m){
                
            //         // direction = mostNegative.Value.direction == TradeDirection.BUY ? TradeDirection.SELL : TradeDirection.BUY;
            //         openTradeBool = true;
            //     // }

            // }

            // We're 40 pips away from the last trade
            // if(tradeObjs.openTrades.Count>1){
            //     if(tradeObjs.openTrades.Any(x=>Math.Abs(x.Value.level-priceObj.ask)*envVariables.GetScalingFactor(priceObj.symbol) < 8))
            //         openTradeBool = false;
            // }  

         

            // Don't open the same direction at the same time
            if(tradeObjs.openTrades.Count == 1 && tradeObjs.openTrades.Any(x=>x.Value.direction == direction)){
                var mostRecentSameDirection = tradeObjs.openTrades.OrderBy(x=>x.Value.direction == direction).Last();
                if((mostRecentSameDirection.Value.level-priceObj.ask)*envVariables.GetScalingFactor(priceObj.symbol) < 5 ){
                    // openTradeBool = false;
                    direction = mostRecentSameDirection.Value.direction == TradeDirection.BUY ? TradeDirection.SELL : TradeDirection.BUY;
                }
            }
           

            // All trades are the same direction, lets mix it up
            if(tradeObjs.openTrades.Count == 2){
                if(tradeObjs.openTrades.All(x=>x.Value.direction == TradeDirection.BUY)){
                    direction = TradeDirection.SELL;
                } else if (tradeObjs.openTrades.All(x=>x.Value.direction == TradeDirection.SELL )){
                    direction = TradeDirection.BUY;
                } 
            }

            // Both are ngeative, lets not make this worse
            // if(tradeObjs.openTrades.Count==2 && tradeObjs.openTrades.All(x=>x.Value.profit < 0)){
            //     openTradeBool = false;
            // }

            // Both are ngeative, lets not make this worse
            // if(tradeObjs.openTrades.Count==1 && tradeObjs.openTrades.All(x=>x.Value.profit < 0)){
            //     var mostNegative = tradeObjs.openTrades.OrderBy(x=>x.Value.profit).First();
            //     direction = mostNegative.Value.direction == TradeDirection.BUY ? TradeDirection.SELL : TradeDirection.BUY;
            //     openTradeBool = true;
            // }

            
            // if(tradeObjs.openTrades.Sum(x=>x.Value.profit) < -25){
            //     // direction = direction == TradeDirection.BUY ? TradeDirection.SELL : TradeDirection.BUY;
            //     // plannedTradingSize = 1;
            //     // plannedLimit = 2;
            // } else if(tradeObjs.openTrades.Count > 0){
            //     return true;
            // }
        } else {
            openTradeBool=true;
        }

        if(!openTradeBool){
            return Task.CompletedTask;
        }
       
        // System.Console.WriteLine("Opening " + tradeObjs.openTrades.Count);
        var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
        {
            size = plannedTradingSize,
            stopDistancePips = plannedStopLoss,
            limitDistancePips = plannedLimit,
        };

        this.requestOpenTrade.Request(openOrderRequest);
            return Task.CompletedTask;
    }
}
