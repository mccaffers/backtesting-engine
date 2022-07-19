using System.Net;
using backtesting_engine.interfaces;

namespace Utilities;

public class EnvironmentVariables : IEnvironmentVariables
{
    // Enables the override of the constructor
    public virtual bool loadFromEnvironmnet { get; } = true;

    public EnvironmentVariables()
    {
        if(!loadFromEnvironmnet){
            return;
        }

        this.operatingEnvironment = Get("operatingEnvironment");

        // Variables for both environments
        this.strategy = Get("strategy");
        this.stopDistanceInPips = Get("stopDistanceInPips");
        this.limitDistanceInPips = Get("limitDistanceInPips");
        this.reportingEnabled = bool.Parse(Get("reportingEnabled"));
        this.scalingFactor = Get("scalingFactor");
        this.tradingSize = Get("tradingSize");
        this.kineticStopLoss = int.Parse(Get("kineticStopLoss"));
        this.elasticPassword = Get("elasticPassword");
        this.elasticUser = Get("elasticUser");
        this.elasticCloudID = Get("elasticCloudID");

        if(operatingEnvironment == "AWS-Lambda"){

        }

        if(operatingEnvironment == "Backtesting"){
            this.symbols = Get("symbols").Split(",");
            this.randomStrategyAmountOfHHLL = int.Parse(Get("randomStrategyAmountOfHHLL"));
            this.kineticLimit = int.Parse(Get("kineticLimit"));
            this.maximumDrawndownPercentage = Get("maximumDrawndownPercentage");
            this.accountEquity = Get("accountEquity");
            this.runID = Get("runID");
            this.symbolFolder = Get("symbolFolder");
            this.s3Bucket = Get("s3Bucket");
            this.s3Path = Get("s3Path");
            this.hostname = Dns.GetHostName();
            this.runIteration = Get("runIteration");
            this.instanceCount = int.Parse(Get("instanceCount"));
            this.tickDataFolder = Path.Combine(Path.GetFullPath("./" + this.symbolFolder));

            this.yearsStart = int.Parse(Get("yearsStart"));
            this.yearsEnd = int.Parse(Get("yearsEnd"));

            this.years = Enumerable.Range(this.yearsStart, this.yearsEnd - this.yearsStart + 1).ToArray(); 

            if(!string.IsNullOrEmpty(Get("doNotCleanUpDataFolder", true))){
                this.doNotCleanUpDataFolder = bool.Parse(Get("doNotCleanUpDataFolder"));
            }
        }


    }

    public static string Get(string envName, bool optional = false)
    {
        var output = Environment.GetEnvironmentVariable(envName) ?? "";
        if (string.IsNullOrEmpty(output) && !optional)
        {
            throw new ArgumentException("Missing environment variable " + envName);
        }
        return output;
    }

    public virtual string strategy { get; init; } = string.Empty;
    public virtual string runID { get; init; } = string.Empty;
    public virtual string symbolFolder { get; init; } = string.Empty;
    public virtual string stopDistanceInPips { get; init; } = string.Empty;
    public virtual string limitDistanceInPips { get; init; } = string.Empty;
    public virtual string elasticPassword { get; init; } = string.Empty;
    public virtual string elasticUser { get; init; } = string.Empty;
    public virtual string elasticCloudID { get; init; } = string.Empty;
    public virtual string accountEquity { get; init; } = string.Empty;
    public virtual string maximumDrawndownPercentage { get; init; } = string.Empty;
    public virtual string s3Bucket { get; init; } = string.Empty;
    public virtual string s3Path { get; init; } = string.Empty;
    public virtual string hostname { get; init; } = string.Empty;
    public virtual string runIteration { get; init; } = string.Empty;
    public virtual string scalingFactor { get; init; } = string.Empty;
    public virtual string tradingSize { get; init; } = string.Empty;
    public virtual int yearsStart { get;init ; }
    public virtual int yearsEnd { get; init; }
    public virtual bool doNotCleanUpDataFolder {get;init;}
    public virtual int kineticStopLoss {get;init;}
    public virtual int kineticLimit {get;init;}
    public virtual int randomStrategyAmountOfHHLL {get; init;}

    // Custom environment variables
    public virtual string tickDataFolder { get; init; } = string.Empty;
    public virtual int instanceCount { get; init; } = 0;
    public virtual bool reportingEnabled { get; init; } = false;
    public virtual string[] symbols { get; init; } = new string[] { string.Empty };
    public virtual int[] years { get; init; } = new int[] {0};
    public string operatingEnvironment { get; init; } = string.Empty;

    public Dictionary<string, decimal> getScalingFactorDictionary()
    {
        var localdictionary = new Dictionary<string, decimal>();
        foreach (var symbol in this.scalingFactor.Split(";"))
        {
            if (string.IsNullOrEmpty(symbol)) {
                continue;
            }

            var scalingFactorArray = symbol.ToString().Split(",");
            decimal sf;
            if (!decimal.TryParse(scalingFactorArray[1], out sf))
            {
                throw new ArgumentException("Cannot read scaling factor of symbol");
            }
            localdictionary.Add(scalingFactorArray[0], sf);
        }
        return localdictionary;
    }

    public decimal GetScalingFactor(string symbol)
    {
        var output = 0m;
        if (!getScalingFactorDictionary().TryGetValue(symbol, out output))
        {
            throw new ArgumentException("Missing scaling factor");
        }
        return output;
    }

}