using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine;


public class CloseOrder : TradingBase, ICloseOrder
{

    readonly IReporting reporting;

    public Dictionary<string, TradeHistoryObject> cache;

    public CloseOrder(IServiceProvider provider, IReporting reporting) : base(provider) {
        this.reporting = reporting;
        cache = new Dictionary<string,TradeHistoryObject>();
    }

    public void PushRequest(PriceObj priceObj){
        foreach(var item in cache){
            if(priceObj.date.Subtract(item.Value.closeDateTime).TotalSeconds > 1){
                // Update profit position
                // item.Value.profit = item.Value.reqObj.profit;
                CloseTrade(item.Value);
                cache.Remove(item.Key);
            }
        }
    }

    // Data Update
    public void Request(RequestObject reqObj, PriceObj priceObj)
    {

        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject();
        tradeHistoryObj.closeLevel = reqObj.close;
        tradeHistoryObj.profit = reqObj.profit;
        tradeHistoryObj.closeDateTime = reqObj.closeDate;
        tradeHistoryObj.runningTime = reqObj.closeDate.Subtract(reqObj.openDate).TotalMinutes;
        tradeHistoryObj.key = reqObj.key;
        tradeHistoryObj.reqObj = reqObj;

        PropertyCopier<RequestObject, TradeHistoryObject>.Copy(reqObj, tradeHistoryObj);

        if(!cache.ContainsKey(tradeHistoryObj.key)){
            cache.Add(tradeHistoryObj.key, tradeHistoryObj);
        }
    }

    private void CloseTrade(TradeHistoryObject tradeHistoryObj){
        var key = DictionaryKeyStrings.CloseTradeKey(tradeHistoryObj.symbol, tradeHistoryObj.openDate, tradeHistoryObj.level);
        this.tradingObjects.tradeHistory.TryAdd(key, tradeHistoryObj);
        this.tradingObjects.accountObj.AddTradeProftOrLoss(tradeHistoryObj.profit);
        this.tradingObjects.openTrades.TryRemove(tradeHistoryObj.key, out _);

        ConsoleLogger.Log(tradeHistoryObj.closeDateTime + "\t" + this.tradingObjects.accountObj.pnl.ToString("0.00") + "\t Closed trade for " + tradeHistoryObj.symbol + "\t" + tradeHistoryObj.profit.ToString("0.00") + "\t" + tradeHistoryObj.direction + "\t" + tradeHistoryObj.level.ToString("0.#####") + "\t" + tradeHistoryObj.closeLevel.ToString("0.#####") + "\t" + tradeHistoryObj.runningTime.ToString("0.00"));

        this.reporting.TradeUpdate(tradeHistoryObj.closeDateTime, tradeHistoryObj.symbol, tradeHistoryObj.profit);
    }
}