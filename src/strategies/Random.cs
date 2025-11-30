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
                            ICloseOrder closeOrder) :
            base(requestOpenTrade, tradeObjs, closeOrder)
    {
        // Empty constructor, just used to inject the dependencies
    }

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj, StrategyDefinition strategy)
    {
        
        var strategyId = strategy.UUID;

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
        var dealReference = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        // Build a request object
        var scale = EnvironmentVariables.GetScalingFactor(priceObj.symbol);
        var size = decimal.Parse(strategy.TRADING_VARIABLES.TRADING_SIZE);
        var stopDistancePips = decimal.Parse(strategy.TRADING_VARIABLES.STOP_DISTANCE_IN_PIPS);
        var limitDistancePips = decimal.Parse(strategy.TRADING_VARIABLES.LIMIT_DISTANCE_IN_PIPS);

               var openOrderRequest = new RequestObject(priceObj: priceObj,
                                        direction:  (TradeDirection)direction,
                                        dealReference: dealReference,
                                        scalingFactor: scale,
                                        stopDistancePips: stopDistancePips,
                                        limitDistancePips: limitDistancePips,
                                        size: size,
                                        strategyId: strategyId);

       
        // Open a trade request
        await requestOpenTrade.Request(openOrderRequest);

        // Surpress CS1998
        // Async method lacks 'await' operators and will run synchronously
        await Task.CompletedTask;

        return;
    }

       public async Task During(PriceObj priceObj)
    {
        await Task.CompletedTask;
    }
    
}
