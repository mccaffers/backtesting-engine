using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine;


public class CloseOrder : TradingBase, ICloseOrder
{

    readonly IReporting reporting;

    public CloseOrder(IServiceProvider provider, IReporting reporting) : base(provider) {
        this.reporting = reporting;
    }

    // Data Update
    public void Request(TradeHistoryObject tradeHistoryObj)
    {
        var key = DictionaryKeyStrings.CloseTradeKey(tradeHistoryObj.symbol, tradeHistoryObj.openDate, tradeHistoryObj.level);
        this.tradingObjects.tradeHistory.TryAdd(key, tradeHistoryObj);
        this.tradingObjects.accountObj.AddTradeProftOrLoss(tradeHistoryObj.profit);
        this.tradingObjects.openTrades.TryRemove(tradeHistoryObj.key, out _);

        ConsoleLogger.Log(tradeHistoryObj.closeDateTime + "\t" + this.tradingObjects.accountObj.pnl.ToString("0.00") + "\t Closed trade for " + tradeHistoryObj.symbol + "\t" + tradeHistoryObj.profit.ToString("0.00") + "\t" + tradeHistoryObj.direction + "\t" + tradeHistoryObj.level.ToString("0.000") + "\t" + tradeHistoryObj.closeLevel.ToString("0.####"));

        this.reporting.TradeUpdate(tradeHistoryObj.closeDateTime, tradeHistoryObj.symbol, tradeHistoryObj.profit);
    }
}