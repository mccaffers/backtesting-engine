using backtesting_engine_operations;

namespace backtesting_engine;

public class AccountObj {
    public decimal openingEquity { get;set; }
    public decimal pnl { get {
        var pl = Program.tradeHistory.Sum(x => x.Value.profit);
        return this.openingEquity + pl + Program.openTrades.Sum(x=>Positions.CalculateProfit(x.Value.close, x.Value));
    }}
}
