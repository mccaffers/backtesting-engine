using System.Collections.Concurrent;
using backtesting_engine.interfaces;
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
    readonly ConcurrentDictionary<string, RequestObject> openTrades;

    public decimal openingEquity { get; init; }
    public decimal maximumDrawndownPercentage { get; init; }
    public decimal tradeHistorySum { get; private set; } = decimal.Zero;

    readonly IEnvironmentVariables envVariables;

    public AccountObj(ConcurrentDictionary<string, RequestObject> openTrades,
                           ConcurrentDictionary<string, TradeHistoryObject> tradeHistory,
                           decimal openingEquity,
                           decimal maximumDrawndownPercentage,
                           IEnvironmentVariables envVariables)
    {

        this.openTrades = openTrades;
        this.openingEquity = openingEquity;
        this.maximumDrawndownPercentage = maximumDrawndownPercentage;
        this.envVariables = envVariables;
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
        return difference * this.envVariables.GetScalingFactor(openTradeObj.symbol) * openTradeObj.size;
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
