using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class BaseStrategy
{
    protected readonly IRequestOpenTrade requestOpenTrade;
    protected readonly IEnvironmentVariables envVariables;
    protected readonly ITradingObjects tradeObjs;
    protected readonly IWebNotification webNotification;
    protected readonly ICloseOrder closeOrder;

    protected BaseStrategy(IRequestOpenTrade requestOpenTrade, ITradingObjects tradeObjs, IEnvironmentVariables envVariables, ICloseOrder closeOrder,  IWebNotification webNotification)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.envVariables = envVariables;
        this.tradeObjs = tradeObjs;
        this.webNotification = webNotification;
        this.closeOrder = closeOrder;
    }

   
}
