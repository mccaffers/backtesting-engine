namespace backtesting_engine.interfaces;

public interface IEnvironmentVariables
{
    string operatingEnvironment { get; init; }
    
    // Both
    string strategy { get; set; }
    string runID { get; init; }
    string symbolFolder { get; init; }
    string stopDistanceInPips { get; set; }
    string limitDistanceInPips { get; set; }
    string elasticPassword { get; init; }
    string elasticUser { get; init; }
    string elasticCloudID { get; init; }
    string accountEquity { get; init; }
    string maximumDrawndownPercentage { get; init; }
    string s3Bucket { get; init; }
    string s3Path { get; init; }
    string hostname { get; init; }
    string runIteration { get; init; }
    string scalingFactor { get; set; }
    string tickDataFolder { get; init; }
    string tradingSize { get; set; }
    int instanceCount { get; init;}
    bool reportingEnabled { get; init; }
    string[] symbols { get; init; }
    int[] years { get; init; }
    int yearsStart {get; init;}
    int yearsEnd { get; init; }
    int kineticStopLoss {get; init;}
    int kineticLimit {get; init;}
    bool doNotCleanUpDataFolder {get;init;}
    bool fasterProcessingBySkippingSomeTickData {get;init;}

    decimal? variableA {get;set;}
    decimal? variableB {get;set;}
    decimal? variableC {get;set;}
    decimal? variableD {get;set;}
    decimal? variableE {get;set;}

    Dictionary<string, decimal> getScalingFactorDictionary();
    decimal GetScalingFactor(string symbol);
    
}