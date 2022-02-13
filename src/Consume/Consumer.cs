using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest;

public interface IConsumer
{
    Task ConsumeAsync(BufferBlock<PriceObj> buffer);
}

public class Consumer : IConsumer
{

    public static ConcurrentDictionary<string, OpenTradeObject> openTrades = new ConcurrentDictionary<string, OpenTradeObject>();
    public static ConcurrentDictionary<string, TradeHistoryObject> tradeHistory = new ConcurrentDictionary<string, TradeHistoryObject >();

    public async Task ConsumeAsync(BufferBlock<PriceObj> buffer)
    {
        while (await buffer.OutputAvailableAsync())
        {
            var priceObj = await buffer.ReceiveAsync();
            RandomStrategy.Invoke();
            OpenTrade.Request(priceObj, openTrades);
            CheckOpenTrades(priceObj);

            // Testing
            // Wait for so many event
            // Open trade randomly
            
        }

        // if we reach here the buffer has been marked Complete()
    }

    public void CheckOpenTrades(PriceObj priceObj){

        var orders = GetOrderBook(priceObj.symbol);

        foreach(var obj in orders){

            var myTradeObj = CalculateProfit(priceObj, obj);
                
            var currentLevel = priceObj.bid;
            if (myTradeObj.direction == "SELL") {
                currentLevel = priceObj.ask;
            }

            if (myTradeObj.direction == "BUY")
            {

                if (currentLevel <= myTradeObj.stopLevel || currentLevel >= myTradeObj.limitLevel)
                {
                    System.Console.WriteLine("threshold for BUY" + " " + myTradeObj.stopLevel + " " + myTradeObj.limitLevel);
                    UpdateTradeHistory(myTradeObj, priceObj);
                }
            }
            else if (myTradeObj.direction == "SELL")
            {
                if (currentLevel >= myTradeObj.stopLevel || currentLevel <= myTradeObj.limitLevel)
                {
                    UpdateTradeHistory(myTradeObj, priceObj);
                }
            }
        }
    }

    public void UpdateTradeHistory(OpenTradeObject openTradeObj, PriceObj priceObj){

        var key =  ""+openTradeObj.symbol+"-"+openTradeObj.openDate+"-"+openTradeObj.level;
        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject();
        
        PropertyCopier<OpenTradeObject, TradeHistoryObject>.Copy(openTradeObj, tradeHistoryObj);
        tradeHistory.TryAdd(key, tradeHistoryObj);
        openTrades.TryRemove(priceObj.symbol, out _);

        System.Console.WriteLine("Closed trade for " + priceObj.symbol + " " + openTradeObj.profit + " " + openTradeObj.direction + " " + openTradeObj.level + " " + openTradeObj.closeLevel);
    }

    public static OpenTradeObject CalculateProfit(PriceObj priceObj, OpenTradeObject openTradeObj){

        if (openTradeObj.direction == "BUY") {
            var currentPrice = priceObj.bid;
            openTradeObj.profit = (decimal)(((currentPrice - (decimal)openTradeObj.level) * openTradeObj.scalingFactor) * openTradeObj.size);
            openTradeObj.closeLevel = currentPrice;
        } else if (openTradeObj.direction == "SELL") {
            var currentPrice = priceObj.ask;
            openTradeObj.profit = (decimal)((((decimal)openTradeObj.level - currentPrice) * openTradeObj.scalingFactor) * openTradeObj.size);
            openTradeObj.closeLevel = currentPrice;
        }

        System.Console.WriteLine(openTradeObj.profit);
        
        openTradeObj.currentDateTime = priceObj.date;
        openTradeObj.runningTime = priceObj.date.Subtract(openTradeObj.openDate).TotalMinutes;

        return openTradeObj;
    }


    public static IEnumerable<OpenTradeObject> GetOrderBook(string symbol){
        return openTrades.Where(x => x.Key.Contains(symbol)).Select(x=>x.Value);
    }

   

}

public class PropertyCopier<TParent, TChild> where TParent : class
                                            where TChild : class
{
    public static void Copy(TParent parent, TChild child)
    {
        var parentProperties = parent.GetType().GetProperties();
        var childProperties = child.GetType().GetProperties();
        foreach (var parentProperty in parentProperties)
        {
            foreach (var childProperty in childProperties)
            {
                if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                {
                    childProperty.SetValue(child, parentProperty.GetValue(parent));
                    break;
                }
            }
        }
    }
}