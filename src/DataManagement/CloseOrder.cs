using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_ingest;
using backtesting_engine_models;
using backtesting_engine_operations;
using Report;
using Utilities;

namespace backtesting_engine;

public static class CloseOrder
{
    // Data Update
    public static void Request(TradeHistoryObject tradeHistoryObj){   

        Program.tradeHistory.TryAdd(DictionaryKeyStrings.CloseTradeKey(tradeHistoryObj), tradeHistoryObj);
        Program.openTrades.TryRemove(tradeHistoryObj.key, out _);

        System.Console.WriteLine("Closed trade for " + tradeHistoryObj.symbol + " " + tradeHistoryObj.profit + " " + tradeHistoryObj.direction + " " + tradeHistoryObj.level + " " + tradeHistoryObj.closeLevel);
        // System.Console.WriteLine("Closed trade for " + priceObj.symbol + " " + tradeHistoryObj.profit + ", Account PL:" + Program.tradeHistory.Sum(x=>x.Value.profit));

        System.Console.WriteLine(Program.accountObj.pnl);
        
        Reporting.TradeUpdate(tradeHistoryObj.closeDateTime, tradeHistoryObj.symbol, tradeHistoryObj.profit);
    }
}