using backtesting_engine;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_operations;

public interface IRequestOpenTrade
{
    void Request(RequestObject reqObj);
}

public class RequestOpenTrade : IRequestOpenTrade
{

    IOpenOrder openOrder;

    public RequestOpenTrade(IOpenOrder openOrder)
    {
        this.openOrder = openOrder;
    }

    // Validation on opening a trade
    public void Request(RequestObject reqObj)
    {

        decimal stopLevel = 0m;
        decimal limitLevel = 0m;
        decimal scalingFactor = EnvironmentVariables.GetScalingFactor(reqObj.symbol);

        decimal slippage = 1m / scalingFactor;
        reqObj.UpdateLevelWithSlippage(slippage);

        if (reqObj.stopDistancePips != 0 && reqObj.limitDistancePips != 0)
        {

            if (reqObj.direction == TradeDirection.SELL)
            {
                stopLevel = reqObj.level + (reqObj.stopDistancePips / scalingFactor);
                limitLevel = reqObj.level - (reqObj.limitDistancePips / scalingFactor);

            }
            else if (reqObj.direction == TradeDirection.BUY)
            {
                stopLevel = reqObj.level - (reqObj.stopDistancePips / scalingFactor);
                limitLevel = reqObj.level + (reqObj.limitDistancePips / scalingFactor);
            }
        }

        reqObj.stopLevel = stopLevel;
        reqObj.limitLevel = limitLevel;

        this.openOrder.Request(reqObj);
    }

}