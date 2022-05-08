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
    public decimal dayRange { get;set;}
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

    public void Invoke(PriceObj priceObj) {

        ohlcList = GenericOHLC.CalculateOHLC(priceObj, TimeSpan.FromDays(1), ohlcList);

        if(ohlcList.Count > 1){

            var dayRange = ohlcList.First().high = ohlcList.First().low;
            ohlcList.RemoveAt(0);

            System.Console.WriteLine(dayRange);

            var vObject = new VolatilityObject(){
                date = priceObj.date,
                symbols = envVariables.symbols,
                runID = envVariables.runID,
                runIteration = int.Parse(envVariables.runIteration),
                strategy = envVariables.strategy,
                dayRange = dayRange
            };

            volatilityList.Add(vObject);
            BatchUpdate();
        }
    }

    private void BatchUpdate(){
        if(DateTime.Now.Subtract(lastBatchUpdate).TotalSeconds <= 5){
            return;
        }
        lastBatchUpdate=DateTime.Now;
        var response = elasticClient.IndexMany(volatilityList, "volatility");
        System.Console.WriteLine(response);
    }
}
