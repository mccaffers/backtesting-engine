using backtesting_engine;

namespace Utilities;

public class DictoinaryKeyStrings
{
    public static string OpenTrade(PriceObj priceObj){
        return priceObj.symbol + "-" + priceObj.date;
    }

    public static string CloseTradeKey(OpenTradeObject openTradeObj){
        return  ""+openTradeObj.symbol+"-"+openTradeObj.openDate+"-"+openTradeObj.level;
    }
}
