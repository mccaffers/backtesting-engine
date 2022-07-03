using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface IRequestOpenTrade
{
    void Request(RequestObject reqObj);
}