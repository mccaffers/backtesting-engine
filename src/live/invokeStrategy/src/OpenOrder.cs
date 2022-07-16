using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine;

public class OpenOrder : IOpenOrder
{
    public OpenOrder(IServiceProvider provider){

    }

    // Data Update
    public void Request(RequestObject reqObj)
    {
        // do nothing
    }
}
