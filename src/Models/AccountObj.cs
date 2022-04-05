using System.Collections.Concurrent;
using backtesting_engine_models;
using backtesting_engine_operations;
using Utilities;

namespace backtesting_engine;

public class AccountObj
{
    readonly ConcurrentDictionary<string, RequestObject> openTrades;
    readonly ConcurrentDictionary<string, TradeHistoryObject> tradeHistory;

    public decimal openingEquity { get; init; } 
    public decimal maximumDrawndownPercentage { get; set; }

    public AccountObj( ConcurrentDictionary<string, RequestObject> openTrades, ConcurrentDictionary<string, TradeHistoryObject> tradeHistory ){
        this.openTrades = openTrades;
        this.tradeHistory = tradeHistory;
        this.openingEquity = decimal.Parse(EnvironmentVariables.accountEquity);
        this.maximumDrawndownPercentage = decimal.Parse(EnvironmentVariables.maximumDrawndownPercentage);
    }

    public decimal pnl
    {
        get
        {
            var pl = tradeHistory.Sum(x => x.Value.profit);
            return this.openingEquity + pl + openTrades.Sum(x => Positions.CalculateProfit(x.Value.close, x.Value));
        }
    }

    public bool hasAccountExceededDrawdownThreshold()
    {
        if(this.maximumDrawndownPercentage==0){
            return false;
        }
        return (this.pnl < this.openingEquity * (1 - (this.maximumDrawndownPercentage / 100)));
    }

}
