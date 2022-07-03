namespace Utilities;

public static class DictionaryKeyStrings
{
    public static string OpenTrade(string symbol, DateTime date){
        return symbol + "-" + date;
    }

    public static string CloseTradeKey(string symbol, DateTime openDate, decimal level){
        return  ""+symbol+"-"+openDate+"-"+level;
    }
}
