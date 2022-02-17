using backtesting_engine;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_operations;

public class Positions {

    public static void Review(PriceObj priceObj){

        foreach(var myTradeObj in GetOrderBook(priceObj.symbol)){

            // System.Console.WriteLine(priceObj.symbol + " " + CalculateProfit(priceObj, myTradeObj) + " " + priceObj.date + " bid:"+ priceObj.bid + " ask:" + priceObj.ask +" open:" + myTradeObj.level + " " + myTradeObj.openDate);
                
            if (myTradeObj.direction == TradeDirection.BUY)
            {
                if (priceObj.bid <= myTradeObj.stopLevel || priceObj.bid >= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(myTradeObj, priceObj);
                }
            }
            else if (myTradeObj.direction == TradeDirection.SELL)
            {
                if (priceObj.ask >= myTradeObj.stopLevel || priceObj.ask <= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(myTradeObj, priceObj);
                }
            }
        }
    }

    public static IEnumerable<RequestObject> GetOrderBook(string symbol){
        return Program.openTrades.Where(x => x.Key.Contains(symbol)).Select(x=>x.Value);
    }

    public static void UpdateTradeHistory(RequestObject reqObj, PriceObj priceObj){

        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject();
        tradeHistoryObj.profit = CalculateProfit(priceObj, reqObj);
        tradeHistoryObj.closeLevel = UpdateCloseLevel(priceObj, reqObj);
        tradeHistoryObj.closeDateTime = priceObj.date;
        tradeHistoryObj.runningTime = priceObj.date.Subtract(reqObj.openDate).TotalMinutes;

        PropertyCopier<RequestObject, TradeHistoryObject>.Copy(reqObj, tradeHistoryObj);
        CloseTrade.Request(priceObj, tradeHistoryObj);
  }

    public static decimal CalculateProfit(PriceObj priceObj, RequestObject openTradeObj){
        var PL = openTradeObj.direction == TradeDirection.BUY ?  priceObj.bid - openTradeObj.level : openTradeObj.level - priceObj.ask;
        return PL * openTradeObj.priceObj.scalingFactor * openTradeObj.size;
    }
    
    public static decimal UpdateCloseLevel(PriceObj priceObj, RequestObject openTradeObj){
        return openTradeObj.direction == TradeDirection.BUY ? priceObj.bid : priceObj.ask;
    }

}
