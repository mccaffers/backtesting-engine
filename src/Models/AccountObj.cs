using backtesting_engine_operations;

namespace backtesting_engine;

public class AccountObj : TradingBase {

    public AccountObj(IServiceProvider provider) : base(provider)
    {
        
    }
    
    public decimal openingEquity { get; init; }
    public decimal maximumDrawndownPercentage {get;set;}

    public decimal pnl { get {
        var pl = this.tradingObjects.tradeHistory.Sum(x => x.Value.profit);
        return this.openingEquity + pl +  this.tradingObjects.openTrades.Sum(x=>Positions.CalculateProfit(x.Value.close, x.Value));
    }}

    public bool hasAccountExceededDrawdownThreshold(){
        return (this.pnl < this.openingEquity*(1-(this.maximumDrawndownPercentage/100)));
    }

}
