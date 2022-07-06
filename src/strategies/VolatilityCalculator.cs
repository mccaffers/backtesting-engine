using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Nest;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;

public class VolatilityCalculator : IStrategy
{
    readonly IEnvironmentVariables envVariables;
    readonly IElasticClient elasticClient;

    public VolatilityCalculator(IEnvironmentVariables envVariables, IElasticClient elasticClient)
    {
        this.envVariables = envVariables;
        this.elasticClient = elasticClient;
    }

    private List<OhlcObject> ohlcList = new List<OhlcObject>();
    private List<VolatilityObject> volatilityList = new List<VolatilityObject>();
    private DateTime lastBatchUpdate = DateTime.Now;
    private decimal lastPrice = decimal.Zero;
    private decimal movement = decimal.Zero;
    private List<decimal> distanceBetweenPriceMoves = new List<decimal>();
    private List<decimal> spreadDistance = new List<decimal>();

    public void Invoke(PriceObj priceObj) {

        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromDays(1), ohlcList);

        if(lastPrice==decimal.Zero){
            lastPrice=priceObj.ask;
        } else if(lastPrice!=priceObj.ask){
            var distance = Math.Abs(lastPrice-priceObj.ask) * envVariables.GetScalingFactor(priceObj.symbol);
            movement+=distance;
            distanceBetweenPriceMoves.Add(distance);
            lastPrice=priceObj.ask;
        }

        var spread = Math.Abs(priceObj.ask - priceObj.bid);
        spreadDistance.Add(spread); 

        // Will only be higher than one count if there is more than one day in the list
        if(ohlcList.Count > 1){

            var dayRange = (ohlcList.First().high - ohlcList.First().low) * envVariables.GetScalingFactor(priceObj.symbol);
            var gap = Math.Abs(ohlcList.First().close - ohlcList.Skip(1).First().open) * envVariables.GetScalingFactor(priceObj.symbol);
            var dayClose = ohlcList.First().close;
            ohlcList.RemoveAt(0);

            var vObject = new VolatilityObject(){
                date = priceObj.date,
                symbols = envVariables.symbols,
                runID = envVariables.runID,
                runIteration = int.Parse(envVariables.runIteration),
                strategy = envVariables.strategy,
                dayRange = dayRange,
                dayClose = dayClose,
                totalMovement = movement,
                dayCloseGap = gap,
                distanceBetweenPriceMoves=distanceBetweenPriceMoves.Average(),
                spreadDistance=spreadDistance.Average() * envVariables.GetScalingFactor(priceObj.symbol)
            };

            distanceBetweenPriceMoves=new List<decimal>();
            spreadDistance=new List<decimal>();
            movement=0m;
            volatilityList.Add(vObject);
            BatchUpdate();
        }
    }

    private void BatchUpdate(){
        if(DateTime.Now.Subtract(lastBatchUpdate).TotalSeconds <= 5){
            return;
        }

        lastBatchUpdate=DateTime.Now;
        elasticClient.IndexMany(volatilityList, "volatility");
        volatilityList= new List<VolatilityObject>();
    }
}
