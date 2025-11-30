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
    protected readonly ITradingObjects tradeObjs;
    protected readonly ICloseOrder closeOrder;

    protected BaseStrategy(IRequestOpenTrade requestOpenTrade, ITradingObjects tradeObjs, ICloseOrder closeOrder)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.tradeObjs = tradeObjs;
        this.closeOrder = closeOrder;
    }

   
}
