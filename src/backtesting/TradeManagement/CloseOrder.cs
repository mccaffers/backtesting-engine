using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using trading_exception;
using Utilities;

namespace backtesting_engine;


public class CloseOrder : TradingBase, ICloseOrder
{

    readonly IReporting reporting;

    public CloseOrder(IServiceProvider provider, IReporting reporting) : base(provider)
    {
        this.reporting = reporting;
    }

    public void Request(RequestObject reqObj, PriceObj priceObj)
    {

        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject
        {
            closeLevel = reqObj.closeLevel,
            profit = reqObj.profit,
            closeDateTime = reqObj.closeDateTime,
            runningTime = reqObj.closeDateTime.Subtract(reqObj.date).TotalMinutes,
            key = reqObj.dealReference,
            reqObj = reqObj
        };

        PropertyCopier<RequestObject, TradeHistoryObject>.Copy(reqObj, tradeHistoryObj);

        if (priceObj.date.Subtract(reqObj.closeDateTime).TotalSeconds > 5){
            // delay start in a task
            // System.Console.WriteLine($"Delaying close of trade for {reqObj.symbol} by 5 seconds to allow for reporting lag");
            Task.Delay(5000).ContinueWith(t => CloseTrade(tradeHistoryObj));
            return;
        } else {
            // Immediately close
            CloseTrade(tradeHistoryObj);
        }
      
    }

    private void CloseTrade(TradeHistoryObject tradeHistoryObj)
    {
        var key = DictionaryKeyStrings.CloseTradeKey(tradeHistoryObj.symbol, tradeHistoryObj.date, tradeHistoryObj.level);
        // System.Console.WriteLine("CloseTrade Key: " + key);
        if (!this.tradingObjects.tradeHistory.TryAdd(key, tradeHistoryObj))
        {
            // System.Console.WriteLine("Failed to add trade to history for key: " + key);
            throw new TradingException($"Failed to add trade to history for key: {key}", "");
        }

        this.tradingObjects.accountObj.AddTradeProftOrLoss(tradeHistoryObj.profit);

        if (!this.tradingObjects.openTrades.Remove(tradeHistoryObj.key, out _))
        {
            throw new TradingException($"Failed to remove open trade for key: {tradeHistoryObj.key}", "");
        }

        ConsoleLogger.Log(tradeHistoryObj.closeDateTime + "\t" + this.tradingObjects.accountObj.pnl.ToString("0.00") + "\t Closed trade for " + tradeHistoryObj.symbol + "\t" + tradeHistoryObj.profit.ToString("0.00") + "\t" + tradeHistoryObj.direction + "\t" + tradeHistoryObj.level.ToString("0.#####") + "\t" + tradeHistoryObj.closeLevel.ToString("0.#####") + "\t" + tradeHistoryObj.runningTime.ToString("0.00"));

 
        this.reporting.TradeUpdate(tradeHistoryObj.closeDateTime,
                                    tradeHistoryObj.symbol,
                                    tradeHistoryObj.profit,
                                    tradeHistoryObj.reqObj?.spread ?? 0,
                                    tradeHistoryObj.runningTime);
    }
}