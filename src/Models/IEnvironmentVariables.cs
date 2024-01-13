namespace backtesting_engine.interfaces;

public interface IEnvironmentVariables
{
    string operatingEnvironment { get; init; }
    
    // Both
    string strategy { get; init; }
    string runID { get; init; }
    string symbolFolder { get; init; }
    string stopDistanceInPips { get; init; }
    string limitDistanceInPips { get; init; }
    string elasticPassword { get; init; }
    string elasticUser { get; init; }
    string elasticCloudID { get; init; }
    string accountEquity { get; init; }
    string maximumDrawndownPercentage { get; init; }
    string s3Bucket { get; init; }
    string s3Path { get; init; }
    string hostname { get; init; }
    string runIteration { get; init; }
    string scalingFactor { get; init; }
    string tickDataFolder { get; init; }
    string tradingSize { get; init; }
    int instanceCount { get; init;}
    bool reportingEnabled { get; init; }
    string[] symbols { get; init; }
    int[] years { get; init; }
    int yearsStart {get; init;}
    int yearsEnd { get; init; }
    int kineticStopLoss {get; init;}
    int kineticLimit {get; init;}
    bool doNotCleanUpDataFolder {get;init;}
    int randomStrategyAmountOfHHLL {get; init;}
    bool fasterProcessingBySkippingSomeTickData {get;init;}

    Dictionary<string, decimal> getScalingFactorDictionary();
    decimal GetScalingFactor(string symbol);
    
}