using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Utilities;

namespace backtesting_engine_strategies;

public class RandomStrategy : IStrategy
{
    readonly IRequestOpenTrade requestOpenTrade;
    readonly IEnvironmentVariables envVariables;
    readonly ITradingObjects tradeObjs;

    IWebNotification webNotification;
    private List<OhlcObject> ohlcList = new List<OhlcObject>();
    private OhlcObject lastItem = new OhlcObject();
    readonly ICloseOrder closeOrder;

    public RandomStrategy(IRequestOpenTrade requestOpenTrade, ITradingObjects tradeObjs, IEnvironmentVariables envVariables,ICloseOrder closeOrder,  IWebNotification webNotification)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.envVariables = envVariables;
        this.tradeObjs = tradeObjs;
        this.webNotification = webNotification;
        this.closeOrder = closeOrder;

    }

    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public async Task Invoke(PriceObj priceObj)
    {

         foreach(var item in this.tradeObjs.openTrades.Where(x => x.Key.Contains(priceObj.symbol)).Select(x => x.Value)){
            if(priceObj.date.Subtract(item.openDate).TotalHours > 1){
                this.closeOrder.Request(item, priceObj);
                return;
            }
        }

        if(priceObj.date.DayOfWeek == DayOfWeek.Sunday)
        {
            return;
        }

        if(priceObj.date.DayOfWeek == DayOfWeek.Friday && priceObj.date.Hour > 14){
            return;
        }

        if(priceObj.date.Hour < 5 || priceObj.date.Hour > 19){
            return;
        }

        var randomInt = new Random().Next(2); 
        // 0 or 1
        TradeDirection direction = TradeDirection.BUY;
        if (randomInt== 0)
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
        return;
    }
}
