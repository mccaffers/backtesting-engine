using System.Collections.Concurrent;
using backtesting_engine_models;

namespace backtesting_engine;

public interface IAccountObj
{
    decimal openingEquity { get; init; }
    decimal maximumDrawndownPercentage { get; init; }
    decimal tradeHistorySum { get; }
    decimal pnl { get; }

    void AddTradeProftOrLoss(decimal input);
    decimal CalculateProfit(decimal level, RequestObject openTradeObj);
    bool hasAccountExceededDrawdownThreshold();
}

public class AccountObj : IAccountObj
{
    readonly Dictionary<string, RequestObject> openTrades;

    public decimal openingEquity { get; init; }
    public decimal maximumDrawndownPercentage { get; init; }
    public decimal tradeHistorySum { get; private set; } = decimal.Zero;

    public readonly ConcurrentDictionary<DateTime, decimal> monthlyAccountPnL = new();
    public decimal accountMax = 0m;
    public decimal accountPercentDrop = 0m;

    public AccountObj(Dictionary<string, RequestObject> openTrades,
                           Dictionary<string, TradeHistoryObject> tradeHistory,
                           decimal openingEquity,
                           decimal maximumDrawndownPercentage)
    {

        this.openTrades = openTrades;
        this.openingEquity = openingEquity;
        this.maximumDrawndownPercentage = maximumDrawndownPercentage;
    }

    public void AddTradeProftOrLoss(decimal input)
    {
        this.tradeHistorySum += input;
    }

    public decimal pnl
    {
        get
        {
            return this.openingEquity + this.tradeHistorySum + openTrades.Sum(x => CalculateProfit(x.Value.closeLevel, x.Value));
        }
    }

    public decimal CalculateProfit(decimal level, RequestObject openTradeObj)
    {
        var difference = openTradeObj.direction == TradeDirection.BUY ? level - openTradeObj.level : openTradeObj.level - level;
        return difference * openTradeObj.scalingFactor * openTradeObj.size;
    }

    public bool hasAccountExceededDrawdownThreshold()
    {
        if (this.maximumDrawndownPercentage == 0)
        {
            return false;
        }
        return (this.pnl < this.openingEquity * (1 - (this.maximumDrawndownPercentage / 100)));
    }

}
