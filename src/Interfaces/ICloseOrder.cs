using backtesting_engine_models;

namespace backtesting_engine.interfaces;

public interface ICloseOrder
{
    void Request(RequestObject reqObj, PriceObj priceObj);
    void PushRequest(PriceObj priceObj);
}
