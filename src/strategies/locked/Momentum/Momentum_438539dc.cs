using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class Momentum_438539dc : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public Momentum_438539dc(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder, webNotification) { }

    private OhlcObject lastItem = new OhlcObject();

    private DateTime lastTraded = DateTime.MinValue;

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(30), ohlcList);

        // Maximum of one trade open at a time
        // Conditional to only invoke the strategy if there are no trades open
        if (tradeObjs.openTrades.Count() >= 1)
        {
            return;
        }
        
        if(priceObj.date.Subtract(lastTraded).TotalHours < 8){
            return;
        }

        
        // Keep 5 hours of history (30*10)
        // Only start trading once you have 5 hours
        if (ohlcList.Count > envVariables.variableA)
        {

            lastTraded=priceObj.date;

            // Get the highest value from all of the OHLC objects
            var distance = (ohlcList.Last().close - ohlcList.First().close) * envVariables.GetScalingFactor(priceObj.symbol);
            var speed = distance / (30 * (envVariables.variableA ?? 0));

            // System.Console.WriteLine(speed);

            // Too slow
            if(Math.Abs(speed) < 1m){
                return;
            }

            var direction = TradeDirection.BUY;
            if(speed > 0){
                direction = TradeDirection.SELL;
            }

            var stopLevel = direction == TradeDirection.BUY ? ohlcList.Min(x => x.low) :
                                                                ohlcList.Max(x => x.high);

            var limitLevel = direction == TradeDirection.BUY ? ohlcList.TakeLast(2).Average(x => x.high): 
                                                                ohlcList.TakeLast(2).Average(x => x.low);

            var stopDistance = direction == TradeDirection.SELL ? (stopLevel - priceObj.bid) : (priceObj.ask - stopLevel);
            stopDistance = stopDistance * envVariables.GetScalingFactor(priceObj.symbol);

            var limitDistance = direction == TradeDirection.BUY ? (limitLevel - priceObj.bid) : (priceObj.ask - limitLevel);
            limitDistance = limitDistance * envVariables.GetScalingFactor(priceObj.symbol);

            if (stopDistance < 10 || limitDistance < 2)
            {
                return;
            }

            var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

            var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
            {
                size = decimal.Parse(envVariables.tradingSize),
                stopDistancePips = stopDistance,
                limitDistancePips = limitDistance
            };

            this.requestOpenTrade.Request(openOrderRequest);

            ohlcList.RemoveAt(0);
        }
        
        // Surpress CS1998
        // Async method lacks 'await' operators and will run synchronously
        await Task.CompletedTask;

    }
}
