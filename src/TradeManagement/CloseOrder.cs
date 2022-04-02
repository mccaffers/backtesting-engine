using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_ingest;
using backtesting_engine_models;
using backtesting_engine_operations;
using Reporting;
using Utilities;

namespace backtesting_engine;

public interface ICloseOrder
{
    void Request(TradeHistoryObject tradeHistoryObj);
}

public class CloseOrder : TradingBase, ICloseOrder
{

    public CloseOrder(IServiceProvider provider) : base(provider) {}

    // Data Update
    public void Request(TradeHistoryObject tradeHistoryObj)
    {

        this.tradingObjects.tradeHistory.TryAdd(DictionaryKeyStrings.CloseTradeKey(tradeHistoryObj), tradeHistoryObj);
        this.tradingObjects.openTrades.TryRemove(tradeHistoryObj.key, out _);

        System.Console.WriteLine(tradeHistoryObj.closeDateTime + "\t" + this.tradingObjects.accountObj.pnl.ToString("0.##") + "\t Closed trade for " + tradeHistoryObj.symbol + "\t" + tradeHistoryObj.profit.ToString("0.##") + "\t" + tradeHistoryObj.direction + "\t" + tradeHistoryObj.level.ToString("0.####") + "\t" + tradeHistoryObj.closeLevel.ToString("0.####"));

        Elastic.TradeUpdate(tradeHistoryObj.closeDateTime, tradeHistoryObj.symbol, tradeHistoryObj.profit);
    }
}