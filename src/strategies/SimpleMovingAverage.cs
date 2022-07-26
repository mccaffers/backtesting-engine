using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;


public class SimpleMovingAverage : IStrategy
{
    readonly IRequestOpenTrade requestOpenTrade;
    readonly IEnvironmentVariables envVariables;
    private List<OhlcObject> ohlcListShortTerm = new List<OhlcObject>();
    private List<OhlcObject> ohlcListLongTerm = new List<OhlcObject>();

    public SimpleMovingAverage(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.envVariables = envVariables;
    }

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public bool Invoke(PriceObj priceObj)
    {

        ohlcListShortTerm = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromHours(4), ohlcListShortTerm);
        ohlcListLongTerm = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromHours(1), ohlcListLongTerm);

        // Keep 30 days of history
        if(ohlcListShortTerm.Count > 50){

            var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);
            
            var direction = TradeDirection.BUY;
            if(priceObj.ask > ohlcListShortTerm.Average(x=>x.close)){
                direction = TradeDirection.SELL;
            }

            // if(ohlcListShortTerm.Average(x=>x.close) > ohlcListLongTerm.Average(x=>x.close)){
            //     direction = TradeDirection.SELL;
            // }

            var openOrderRequest = new RequestObject(priceObj, direction, envVariables, key)
            {
                size = decimal.Parse(envVariables.tradingSize),
                limitDistancePips = 100,
                stopDistancePips = 100
            
            };

            this.requestOpenTrade.Request(openOrderRequest);

            ohlcListShortTerm.RemoveAt(0);
        }
        return true;
    }
}
