using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface ICloseOrder
{
    void Request(TradeHistoryObject tradeHistoryObj);
}
