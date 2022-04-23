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

    readonly IOpenOrder openOrder;
    readonly IEnvironmentVariables envVariables;

    public RequestOpenTrade(IOpenOrder openOrder, IEnvironmentVariables envVariables)
    {
        this.openOrder = openOrder;
        this.envVariables = envVariables;
    }

    // Validation on opening a trade
    public void Request(RequestObject reqObj)
    {

        // decimal stopLevel = 0m;
        // decimal limitLevel = 0m;
        // decimal scalingFactor = envVariables.GetScalingFactor(reqObj.symbol);

        

        // if (reqObj.stopDistancePips != 0 && reqObj.limitDistancePips != 0)
        // {

        //     if (reqObj.direction == TradeDirection.SELL)
        //     {
        //         stopLevel = reqObj.level + (reqObj.stopDistancePips / scalingFactor);
        //         limitLevel = reqObj.level - (reqObj.limitDistancePips / scalingFactor);

        //     }
        //     else if (reqObj.direction == TradeDirection.BUY)
        //     {
        //         stopLevel = reqObj.level - (reqObj.stopDistancePips / scalingFactor);
        //         limitLevel = reqObj.level + (reqObj.limitDistancePips / scalingFactor);
        //     }
        // }

        // reqObj.stopLevel = stopLevel;
        // reqObj.limitLevel = limitLevel;

        this.openOrder.Request(reqObj);
    }

}