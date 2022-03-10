using System.Net;

namespace Utilities;

public static class EnvironmentVariables
{

    static EnvironmentVariables()
    { }

    private static string Get(string envName, bool option = false){
        var output = Environment.GetEnvironmentVariable(envName);
        if(output == null && !option){
            throw new ArgumentException("Missing environment variable " + envName);
        }
        return output ?? "";
    }

    public static string strategy {get;} = Get("strategy");
    public static string runID {get;} = Get("runID");
    public static string localFolderPath { get; } = Get("localFolderPath");
    public static string elasticPassword {get;} = Get("elasticPassword");
    public static string elasticUser {get;} = Get("elasticUser");
    public static string elasticCloudID {get;} = Get("elasticCloudID");
    public static string accountEquity {get;} = Get("accountEquity");
    public static string maximumDrawndownPercentage {get;} = Get("maximumDrawndownPercentage");
    public static string s3Bucket {get;} = Get("s3Bucket");
    public static string s3Path {get;} = Get("s3Path");
    public static string hostname {get;} = Dns.GetHostName();
    
    // Custom environment variables
    public static string tickDataFolder = Path.Combine(Path.GetFullPath("./" + localFolderPath));
    public static bool reportingEnabled {get;} = bool.Parse(Get("reportingEnabled"));
    public static string[] symbols {get;} = Get("symbols").Split(",");
    public static int[] years {get;} =  Get("years").Split(',').Select(n => Convert.ToInt32(n)).ToArray();

    public static Dictionary<string, decimal> scalingFactor { get; } = PopulateDictionary();

    private static Dictionary<string, decimal> PopulateDictionary(){
        Dictionary<string, decimal> localdictionary = new Dictionary<string, decimal>();
        foreach(var symbol in symbols){
            decimal sf;
            if(!decimal.TryParse(Get(symbol+"_SF"), out sf)){
                throw new ArgumentException("Cannot read scaling factor of symbol");
            }
            localdictionary.Add(symbol, sf);
        }
        return localdictionary;
    }

}
