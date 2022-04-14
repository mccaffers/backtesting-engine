using System.Net;

namespace Utilities;

public interface IEnvironmentVariables
{
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
    bool reportingEnabled { get; init; }
    string[] symbols { get; init; }
    int[] years { get; init; }
    
    Dictionary<string, decimal> getScalingFactorDictionary();
    decimal GetScalingFactor(string symbol);
}

public class EnvironmentVariables : IEnvironmentVariables
{
    // Enables the override of the constructor
    public virtual bool loadFromEnvironmnet { get; } = true;

    public EnvironmentVariables()
    {
        if(!loadFromEnvironmnet){
            return;
        }

        this.strategy = Get("strategy");
        this.runID = Get("runID");
        this.symbolFolder = Get("symbolFolder");
        this.stopDistanceInPips = Get("stopDistanceInPips");
        this.limitDistanceInPips = Get("limitDistanceInPips");
        this.elasticPassword = Get("elasticPassword");
        this.elasticUser = Get("elasticUser");
        this.elasticCloudID = Get("elasticCloudID");
        this.accountEquity = Get("accountEquity");
        this.maximumDrawndownPercentage = Get("maximumDrawndownPercentage");
        this.s3Bucket = Get("s3Bucket");
        this.s3Path = Get("s3Path");
        this.hostname = Dns.GetHostName();
        this.runIteration = Get("runIteration");
        this.tickDataFolder = Path.Combine(Path.GetFullPath("./" + this.symbolFolder));
        this.reportingEnabled = bool.Parse(Get("reportingEnabled"));
        this.symbols = Get("symbols").Split(",");
        this.years = Get("years").Split(',').Select(n => Convert.ToInt32(n)).ToArray();
        this.scalingFactor = Get("scalingFactor");
    }

    static string Get(string envName)
    {
        var output = Environment.GetEnvironmentVariable(envName);
        if (string.IsNullOrEmpty(output))
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

    // Custom environment variables
    public virtual string tickDataFolder { get; init; } = string.Empty;
    public virtual bool reportingEnabled { get; init; } = false;
    public virtual string[] symbols { get; init; } = new string[] { string.Empty };
    public virtual int[] years { get; init; } = new int[] {0};

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
