using backtesting_engine_models;

namespace backtesting_engine.interfaces;

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