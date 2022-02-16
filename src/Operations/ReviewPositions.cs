using backtesting_engine;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_operations;

public class Positions {

    public static void Review(PriceObj priceObj){

        foreach(var obj in GetOrderBook(priceObj.symbol)){

            var myTradeObj = obj.Value;

            System.Console.WriteLine(priceObj.symbol + " " + CalculateProfit(priceObj, myTradeObj));
                
            if (myTradeObj.direction == TradeDirection.BUY)
            {
                if (priceObj.bid <= myTradeObj.stopLevel || priceObj.bid >= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(obj.Key, myTradeObj, priceObj);
                }
            }
            else if (myTradeObj.direction == TradeDirection.SELL)
            {
                if (priceObj.ask >= myTradeObj.stopLevel || priceObj.ask <= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(obj.Key, myTradeObj, priceObj);
                }
            }
        }
    }

    public static IEnumerable<KeyValuePair<string, backtesting_engine.OpenTradeObject>> GetOrderBook(string symbol){
        return Program.openTrades.Where(x => x.Key.Contains(symbol));
    }

    public static void UpdateTradeHistory(string key, OpenTradeObject openTradeObj, PriceObj priceObj){

        // Calculate Profit
        openTradeObj.profit = CalculateProfit(priceObj, openTradeObj);
        openTradeObj.closeLevel = UpdateCloseLevel(priceObj,openTradeObj);
        openTradeObj.currentDateTime = priceObj.date;
        openTradeObj.runningTime = priceObj.date.Subtract(openTradeObj.openDate).TotalMinutes;

        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject();
        
        PropertyCopier<OpenTradeObject, TradeHistoryObject>.Copy(openTradeObj, tradeHistoryObj);

        Program.tradeHistory.TryAdd(DictoinaryKeyStrings.CloseTradeKey(openTradeObj), tradeHistoryObj);
        Program.openTrades.TryRemove(key, out _);

        System.Console.WriteLine("Closed trade for " + priceObj.symbol + " " + openTradeObj.profit + " " + openTradeObj.direction + " " + openTradeObj.level + " " + openTradeObj.closeLevel);
    }

    public static decimal CalculateProfit(PriceObj priceObj, OpenTradeObject openTradeObj){
        var PL = openTradeObj.direction == TradeDirection.BUY ?  priceObj.bid - openTradeObj.level : openTradeObj.level - priceObj.ask;
        return PL * openTradeObj.scalingFactor * openTradeObj.size;
    }
    
    public static decimal UpdateCloseLevel(PriceObj priceObj, OpenTradeObject openTradeObj){
        return openTradeObj.direction == TradeDirection.BUY ? priceObj.bid : priceObj.ask;
    }

}
