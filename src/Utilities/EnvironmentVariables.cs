using System.Net;

namespace Utilities;

public static class EnvironmentVariables
{

    private static string Get(string envName, bool optional = false){
        var output = Environment.GetEnvironmentVariable(envName);
        if(output == null && !optional){
            throw new ArgumentException("Missing environment variable " + envName);
        }
        return output ?? "";
    }

    public static string strategy {get;} = Get("strategy");
    public static string runID {get;} = Get("runID");
    public static string symbolFolder { get; } = Get("symbolFolder", true);
    public static string stopDistanceInPips { get; } = Get("stopDistanceInPips");
    public static string limitDistanceInPips { get; } = Get("limitDistanceInPips");
    public static string elasticPassword {get;} = Get("elasticPassword");
    public static string elasticUser {get;} = Get("elasticUser");
    public static string elasticCloudID {get;} = Get("elasticCloudID");
    public static string accountEquity {get;} = Get("accountEquity");
    public static string maximumDrawndownPercentage {get;} = Get("maximumDrawndownPercentage");
    public static string s3Bucket {get;} = Get("s3Bucket");
    public static string s3Path {get;} = Get("s3Path");
    public static string hostname {get;} = Dns.GetHostName();
    
    // Custom environment variables
    public static string tickDataFolder {get;} = Path.Combine(Path.GetFullPath("./" + symbolFolder));
    public static bool reportingEnabled {get;} = bool.Parse(Get("reportingEnabled"));
    public static string[] symbols {get;} = Get("symbols").Split(",");
    public static int[] years {get;} =  Get("years").Split(',').Select(n => Convert.ToInt32(n)).ToArray();

    private static Dictionary<string, decimal> scalingFactorDictionary { get; } = PopulateDictionary();

    private static Dictionary<string, decimal> PopulateDictionary(){
        Dictionary<string, decimal> localdictionary = new Dictionary<string, decimal>();

        foreach(var symbol in Get("scalingFactor").Split(";")){
            if(string.IsNullOrEmpty(symbol)){
                continue;
            }
            var sfString = symbol.ToString().Split(",");
            decimal sf;
            if(!decimal.TryParse(sfString[1], out sf)){
                throw new ArgumentException("Cannot read scaling factor of symbol");
            }
            localdictionary.Add(sfString[0], sf);
        }
        return localdictionary;
    }
    public static decimal GetScalingFactor(string symbol){
        var scalingFactor = 0m;
        var output = scalingFactorDictionary.TryGetValue(symbol, out scalingFactor);

        if(!output){
            throw new ArgumentException("Missing scaling factor");
        }
        return scalingFactor;
    }

}
