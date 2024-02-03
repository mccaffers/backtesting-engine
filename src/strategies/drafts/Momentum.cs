using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;

// https://mccaffers.com/randomly_trading/
public class Momentum : BaseStrategy, IStrategy
{

    private List<OhlcObject> ohlcList = new List<OhlcObject>();
    private OhlcObject lastItem = new OhlcObject();
    private DateTime lastReportedEvent = DateTime.MinValue;
    private PriceObj? earlyMorningPrice;
    private PriceObj? lateMorningPrice;

    public Momentum(  IRequestOpenTrade requestOpenTrade, 
                            ITradingObjects tradeObjs, 
                            IEnvironmentVariables envVariables,
                            ICloseOrder closeOrder,
                            IWebNotification webNotification ) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder,  webNotification) {}

    
    public async Task Invoke(PriceObj priceObj)
    {

    foreach(var item in this.tradeObjs.openTrades.Where(x => x.Key.Contains(priceObj.symbol)).Select(x => x.Value)){
            if(priceObj.date.Subtract(item.openDate).TotalHours > 1 && item.profit > 10){
                this.closeOrder.Request(item, priceObj);
                return;
            }

             if(priceObj.date.Subtract(item.openDate).TotalHours > 3 && item.profit > 5){
                this.closeOrder.Request(item, priceObj);
                return;
            }
              if(priceObj.date.Subtract(item.openDate).TotalHours > 4 && item.profit > 0){
                this.closeOrder.Request(item, priceObj);
                return;
            }
        }
      // Maximum of one open trade
        // TODO Make configurable
        if (tradeObjs.openTrades.Count() >= 1)
        {
           return;
        }
        
        if(lastReportedEvent == DateTime.MinValue){
            lastReportedEvent = priceObj.date;
            return;
        }


        var randomInt = new Random().Next(8); 
        
        // TradeDirection direction = TradeDirection.BUY;

        if (randomInt==0)
        { 
            return;
        }

        // if(priceObj.date.DayOfWeek == DayOfWeek.Sunday)
        // {
        //     return;
        // }

        if(priceObj.date.Hour < 6 || priceObj.date.Hour > 23){
            return;
        }

         if(priceObj.date.DayOfWeek == DayOfWeek.Sunday || priceObj.date.DayOfWeek == DayOfWeek.Friday ){
            return;
        }

        // The day has changed
        // if(lastReportedEvent.Hour != priceObj.date.Hour) {
            // After 7 am, lets start recording
            if(earlyMorningPrice == null){
                earlyMorningPrice=priceObj;
            }
     
            // After 7 am, lets start recording
            if(earlyMorningPrice?.date.Subtract(priceObj.date).TotalMinutes < -30 && lateMorningPrice == null) {
                lateMorningPrice=priceObj;
            }

            if(earlyMorningPrice!=null && lateMorningPrice != null){
                lastReportedEvent=priceObj.date.Date;
                var diff = 1 - (earlyMorningPrice.bid / lateMorningPrice.bid);
                
                if(Math.Abs(diff*1000) >= 1 ){

                    TradeDirection dir = TradeDirection.SELL;
                    if(diff > 0) {
                        dir = TradeDirection.BUY;
                    }
                    var size = decimal.Parse(envVariables.tradingSize);

 
                    OpenTrade(priceObj, dir, size);
                    
                    
                }

                earlyMorningPrice=null;
                lateMorningPrice=null;
            }
            
        // }

        
        // Surpress CS1998
        // Async method lacks 'await' operators and will run synchronously
        await Task.CompletedTask;
        
        return;
    }

    private void OpenTrade(PriceObj priceObj, TradeDirection direction, decimal size){
     
        var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
        {
            size = size ,
            stopDistancePips = decimal.Parse(envVariables.stopDistanceInPips),
            limitDistancePips = decimal.Parse(envVariables.limitDistanceInPips),
        };

        this.requestOpenTrade.Request(openOrderRequest);
    }
}
