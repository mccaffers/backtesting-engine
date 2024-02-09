using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

// Read about this strategy on https://mccaffers.com/randomly_trading/
public class RandomStrategy : BaseStrategy, IStrategy
{
    // Dependency injection pulls a number of classes
    // This allows the reuse in different environments (eg. backtesting and live)
    public RandomStrategy(IRequestOpenTrade requestOpenTrade,
                            ITradingObjects tradeObjs,
                            IEnvironmentVariables envVariables,
                            ICloseOrder closeOrder,
                            IWebNotification webNotification) :
            base(requestOpenTrade, tradeObjs, envVariables, closeOrder, webNotification)
    {
        // Empty constructor, just used to inject the dependencies
    }

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {
        
        // Maximum of one trade open at a time
        // Conditional to only invoke the strategy if there are no trades open
        if (tradeObjs.openTrades.Count() >= 1)
        {
            return;
        }
         
        // Generate a random number betwee 0 and 1
        var randomInt = new Random().Next(2);

        // Default to a BUY direction        
        TradeDirection direction = TradeDirection.BUY;

        // Depending on the random integer, switch to SELL
        if (randomInt == 0)
        {
            direction = TradeDirection.SELL;
        }

        // Generate a key for the new trade
        var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        // Build a request object
        var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
        {
            size = decimal.Parse(envVariables.tradingSize),
            stopDistancePips = decimal.Parse(envVariables.stopDistanceInPips),
            limitDistancePips = decimal.Parse(envVariables.limitDistanceInPips),
        };

       
        // Open a trade request
        requestOpenTrade.Request(openOrderRequest);

        // Surpress CS1998
        // Async method lacks 'await' operators and will run synchronously
        await Task.CompletedTask;

        return;
    }
}
