namespace Utilities;

public static class DictionaryKeyStrings
{
    public static string OpenTrade(string symbol, DateTime date){
        var randomInt = new Random().Next(200); 
        return symbol + "-" + date + "-" + randomInt;
    }

    public static string CloseTradeKey(string symbol, DateTime openDate, decimal level){
        return  ""+symbol+"-"+openDate+"-"+level;
    }
}
