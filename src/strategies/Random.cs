using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;

// https://mccaffers.com/randomly_trading/
public class RandomStrategy : BaseStrategy, IStrategy
{

    private List<OhlcObject> ohlcList = new List<OhlcObject>();
    private OhlcObject lastItem = new OhlcObject();

    public RandomStrategy(  IRequestOpenTrade requestOpenTrade, 
                            ITradingObjects tradeObjs, 
                            IEnvironmentVariables envVariables,
                            ICloseOrder closeOrder,
                            IWebNotification webNotification ) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder,  webNotification) {}

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {
        // Maximum of one open trade
        // TODO Make configurable
        if (tradeObjs.openTrades.Count() >= 1)
        {
           return;
        }

        var randomInt = new Random().Next(2); 
        
        TradeDirection direction = TradeDirection.BUY;

        if (randomInt==0)
        { 
            direction = TradeDirection.SELL;
        }

        var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
        {
            size = decimal.Parse(envVariables.tradingSize),
            stopDistancePips = decimal.Parse(envVariables.stopDistanceInPips),
            limitDistancePips = decimal.Parse(envVariables.limitDistanceInPips),
        };

        this.requestOpenTrade.Request(openOrderRequest);
        
        // Surpress CS1998
        // Async method lacks 'await' operators and will run synchronously
        await Task.CompletedTask;
        
        return;
    }
}
