using System.Diagnostics.CodeAnalysis;

namespace Utilities;

public static class DictionaryKeyStrings
{
    [SuppressMessage("Sonar Code Smell", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive", Justification = "Random function has no security use")]
    public static string OpenTrade(string symbol, DateTime date){
        var randomInt = new Random().Next(200); 
        return symbol + "-" + date + "-" + randomInt;
    }

    public static string CloseTradeKey(string symbol, DateTime openDate, decimal level){
        return  ""+symbol+"-"+openDate+"-"+level;
    }
}
