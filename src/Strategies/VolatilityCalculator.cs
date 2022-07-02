using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Nest;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_strategies;

public class VolatilityObject {
    public DateTime date {get;set;}
    public string[] symbols {get;set;} = new string[]{};
    public string runID {get;set;} = string.Empty;
    public int runIteration {get;set;}
    public string strategy {get;set;} = string.Empty;
    public decimal dayClose { get;set;}
    public decimal dayRange { get;set;}
    public decimal totalMovement { get;set;}
    public decimal distanceBetweenPriceMoves { get;set;}
    public decimal dayCloseGap { get;set;}
    public decimal spreadDistance {get;set;}
}

public class VolatilityCalculator : IStrategy
{
    readonly IRequestOpenTrade requestOpenTrade;
    readonly IEnvironmentVariables envVariables;
    readonly IElasticClient elasticClient;

    public VolatilityCalculator(IRequestOpenTrade requestOpenTrade, IEnvironmentVariables envVariables, IElasticClient elasticClient)
    {
        this.requestOpenTrade = requestOpenTrade;
        this.envVariables = envVariables;
        this.elasticClient = elasticClient;
    }

    private List<OHLCObject> ohlcList = new List<OHLCObject>();
    private List<VolatilityObject> volatilityList = new List<VolatilityObject>();
    private DateTime lastBatchUpdate = DateTime.Now;
    private decimal lastPrice = decimal.Zero;
    private decimal movement = decimal.Zero;
    private List<decimal> distanceBetweenPriceMoves = new List<decimal>();
    private List<decimal> spreadDistance = new List<decimal>();

    private decimal lastBid = decimal.Zero;
    private decimal lastAsk = decimal.Zero;

    public void Invoke(PriceObj priceObj) {

        ohlcList = GenericOHLC.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromDays(1), ohlcList);

        if(lastPrice==decimal.Zero){
            lastPrice=priceObj.ask;
        } else if(lastPrice!=priceObj.ask){
            var distance = Math.Abs(lastPrice-priceObj.ask) * envVariables.GetScalingFactor(priceObj.symbol);
            movement+=distance;
            distanceBetweenPriceMoves.Add(distance);
            lastPrice=priceObj.ask;
        }

        if(lastBid != priceObj.bid || lastAsk != priceObj.ask){
            if(lastBid!=priceObj.bid){
                lastBid=priceObj.bid;
            }
            if(lastAsk!=priceObj.ask){
                lastAsk=priceObj.ask;
            }
            var spread = Math.Abs(lastAsk - lastBid);
            spreadDistance.Add(spread); 
        }

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

            System.Console.WriteLine(vObject.spreadDistance);

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
