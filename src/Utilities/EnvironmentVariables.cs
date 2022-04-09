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
    string tickDataFolder { get; init; }
    bool reportingEnabled { get; init; }
    string[] symbols { get; init; }
    int[] years { get; init; }
    Dictionary<string, decimal> scalingFactorDictionary { get; init; }

    decimal GetScalingFactor(string symbol);
}

public class EnvironmentVariables : IEnvironmentVariables
{

    public EnvironmentVariables()
    {
        this.strategy = Get("strategy");
        this.runID = Get("runID");
        this.symbolFolder = Get("symbolFolder", true);
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
        this.scalingFactorDictionary = PopulateDictionary();
    }

    private static string Get(string envName, bool optional = false)
    {
        var output = Environment.GetEnvironmentVariable(envName);
        if (output == null && !optional)
        {
            throw new ArgumentException("Missing environment variable " + envName);
        }
        return output ?? "";
    }

    public string strategy { get; init; }
    public string runID { get; init; }
    public string symbolFolder { get; init; }
    public string stopDistanceInPips { get; init; }
    public string limitDistanceInPips { get; init; }
    public string elasticPassword { get; init; }
    public string elasticUser { get; init; }
    public string elasticCloudID { get; init; }
    public string accountEquity { get; init; }
    public string maximumDrawndownPercentage { get; init; }
    public string s3Bucket { get; init; }
    public string s3Path { get; init; }
    public string hostname { get; init; }
    public string runIteration { get; init; }

    // Custom environment variables
    public string tickDataFolder { get; init; }
    public bool reportingEnabled { get; init; }
    public string[] symbols { get; init; }
    public int[] years { get; init; }
    public Dictionary<string, decimal> scalingFactorDictionary { get; init; }

    private Dictionary<string, decimal> PopulateDictionary()
    {
        Dictionary<string, decimal> localdictionary = new Dictionary<string, decimal>();

        foreach (var symbol in Get("scalingFactor").Split(";"))
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

    public decimal GetScalingFactor(string symbol)
    {
        var scalingFactor = 0m;
        var output = scalingFactorDictionary.TryGetValue(symbol, out scalingFactor);

        if (!output)
        {
            throw new ArgumentException("Missing scaling factor");
        }
        return scalingFactor;
    }

}
