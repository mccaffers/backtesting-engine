using backtesting_engine;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_operations;

public class Positions {


    public static void CloseAll(){
        foreach(var item in Program.openTrades){
            UpdateTradeHistory(item.Value);
        }
    }

    public static decimal GetOpenPL(){
        return Program.openTrades.Sum(x=>CalculateProfit(x.Value.close, x.Value));
    }

    public static void Review(PriceObj priceObj){

        foreach(var myTradeObj in GetOrderBook(priceObj.symbol)){

            myTradeObj.UpdateClose(priceObj);

            if (myTradeObj.direction == TradeDirection.BUY)
            {
                if (priceObj.bid <= myTradeObj.stopLevel || priceObj.bid >= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(myTradeObj);
                }
            }
            else if (myTradeObj.direction == TradeDirection.SELL)
            {
                if (priceObj.ask >= myTradeObj.stopLevel || priceObj.ask <= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(myTradeObj);
                }
            }
        }
    }

    public static IEnumerable<RequestObject> GetOrderBook(string symbol){
        return Program.openTrades.Where(x => x.Key.Contains(symbol)).Select(x=>x.Value);
    }

    public static void UpdateTradeHistory(RequestObject reqObj){

        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject();
        tradeHistoryObj.closeLevel = reqObj.close;
        tradeHistoryObj.profit = reqObj.profit;
        tradeHistoryObj.closeDateTime = reqObj.closeDate;
        tradeHistoryObj.runningTime = reqObj.closeDate.Subtract(reqObj.openDate).TotalMinutes;

        PropertyCopier<RequestObject, TradeHistoryObject>.Copy(reqObj, tradeHistoryObj);
        CloseTrade.Request(tradeHistoryObj);
  }

    public static decimal CalculateProfit(decimal level, RequestObject openTradeObj){
        var difference = openTradeObj.direction == TradeDirection.BUY ? level - openTradeObj.level : openTradeObj.level - level;
        return difference * openTradeObj.priceObj.scalingFactor * openTradeObj.size;
    }
    
}
