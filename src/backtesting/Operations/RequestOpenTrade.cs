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

    public RequestOpenTrade(IOpenOrder openOrder)
    {
        this.openOrder = openOrder;
    }

    public void Request(RequestObject reqObj)
    {

        // Open trade validation
        // If hours are between
        // If X amount of trades are open

        this.openOrder.Request(reqObj);
    }

}