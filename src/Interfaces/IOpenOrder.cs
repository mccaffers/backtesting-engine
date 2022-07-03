using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface IOpenOrder
{
    void Request(RequestObject reqObj);
}