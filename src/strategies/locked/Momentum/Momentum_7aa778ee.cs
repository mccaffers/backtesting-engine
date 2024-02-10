using System.Diagnostics.CodeAnalysis;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class Momentum_7aa778ee : BaseStrategy, IStrategy
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();

    public Momentum_7aa778ee(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, ITradingObjects tradeObjs, ICloseOrder closeOrder, IWebNotification webNotification) : base(requestOpenTrade, tradeObjs, envVariables, closeOrder, webNotification) { }

    private OhlcObject lastItem = new OhlcObject();

    private DateTime lastTraded = DateTime.MinValue;

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

        var totalOHLCCount = decimal.ToInt32(envVariables.variableA ?? throw new Exception());
        var period = envVariables.variableB ?? throw new Exception();
        var maxSpeed = envVariables.variableC ?? throw new Exception();
        var maxStopValue = 1000;
        var takeLast = decimal.ToInt32(envVariables.variableE ?? throw new Exception());

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(Decimal.ToDouble(period)), ohlcList);

        // Maximum of one trade open at a time
        // Conditional to only invoke the strategy if there are no trades open
        if (tradeObjs.openTrades.Count() >= 1)
        {
            return;
        }
        
        if(priceObj.date.Subtract(lastTraded).TotalHours < 24){
            return;
        }

        // Keep 5 hours of history (30*10)
        // Only start trading once you have 5 hours
        if (ohlcList.Count >= envVariables.variableA)
        {

            // System.Console.WriteLine(ohlcList.Count);
            var localOHLCList = ohlcList.TakeLast(totalOHLCCount);

            lastTraded=priceObj.date;

            // Get the highest value from all of the OHLC objects
            var distance = (ohlcList.Last().close - ohlcList.First().close) * envVariables.GetScalingFactor(priceObj.symbol);
            var speed = distance / (period * (envVariables.variableA ?? 0));

            // Too slow
            if(Math.Abs(speed) < maxSpeed){
                return;
            }

            var direction = TradeDirection.BUY;
            if(speed > 0){
                direction = TradeDirection.SELL;
            }

            var stopLevel = direction == TradeDirection.BUY ? ohlcList.Min(x => x.low) :
                                                                ohlcList.Max(x => x.high);

            var limitLevel = direction == TradeDirection.BUY ? ohlcList.TakeLast(takeLast).Average(x => x.high): 
                                                                ohlcList.TakeLast(takeLast).Average(x => x.low);

            var stopDistance = direction == TradeDirection.SELL ? (stopLevel - priceObj.bid) : (priceObj.ask - stopLevel);
            stopDistance = stopDistance * envVariables.GetScalingFactor(priceObj.symbol);

            var limitDistance = direction == TradeDirection.BUY ? (limitLevel - priceObj.bid) : (priceObj.ask - limitLevel);
            limitDistance = limitDistance * envVariables.GetScalingFactor(priceObj.symbol);

            if (stopDistance < 10 || limitDistance < 2)
            {
                return;
            }

            var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);
            var size = decimal.Parse(envVariables.tradingSize);

            if(stopDistance > maxStopValue) {
                stopDistance = maxStopValue;
            }

            var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
            {
                size = size,
                stopDistancePips = stopDistance,
                limitDistancePips = limitDistance
            };

            this.requestOpenTrade.Request(openOrderRequest);

            ohlcList.RemoveAll( x=>x.complete);
        }
        
        // Surpress CS1998
        // Async method lacks 'await' operators and will run synchronously
        await Task.CompletedTask;

    }
}
