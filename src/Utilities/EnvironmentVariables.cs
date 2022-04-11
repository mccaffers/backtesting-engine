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
    Dictionary<string, decimal> scalingFactorDictionary { get; }

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

    protected string Get(string envName)
    {
        var output = Environment.GetEnvironmentVariable(envName);
        if (string.IsNullOrEmpty(output))
        {
            throw new ArgumentException("Missing environment variable " + envName);
        }
        return output ?? "";
    }

    public virtual string strategy { get; init; }
    public virtual string runID { get; init; }
    public virtual string symbolFolder { get; init; }
    public virtual string stopDistanceInPips { get; init; }
    public virtual string limitDistanceInPips { get; init; }
    public virtual string elasticPassword { get; init; }
    public virtual string elasticUser { get; init; }
    public virtual string elasticCloudID { get; init; }
    public virtual string accountEquity { get; init; }
    public virtual string maximumDrawndownPercentage { get; init; }
    public virtual string s3Bucket { get; init; }
    public virtual string s3Path { get; init; }
    public virtual string hostname { get; init; }
    public virtual string runIteration { get; init; }
    public virtual string scalingFactor { get; init; }

    // Custom environment variables
    public virtual string tickDataFolder { get; init; }
    public virtual bool reportingEnabled { get; init; }
    public virtual string[] symbols { get; init; }
    public virtual int[] years { get; init; }

    public Dictionary<string, decimal> scalingFactorDictionary
    {
        get
        {
            Dictionary<string, decimal> localdictionary = new Dictionary<string, decimal>();

            foreach (var symbol in this.scalingFactor.Split(";"))
            {
                if (string.IsNullOrEmpty(symbol))
                {
                    continue;
                }
                var sfString = symbol.ToString().Split(",");
                decimal sf;
                if (!decimal.TryParse(sfString[1], out sf))
                {
                    throw new ArgumentException("Cannot read scaling factor of symbol");
                }
                localdictionary.Add(sfString[0], sf);
            }
            return localdictionary;
        }
    }

    public decimal GetScalingFactor(string symbol)
    {
        var scalingFactor = 0m;
        var output = this.scalingFactorDictionary.TryGetValue(symbol, out scalingFactor);

        if (!output)
        {
            throw new ArgumentException("Missing scaling factor");
        }
        return scalingFactor;
    }

}
