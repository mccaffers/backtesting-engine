using backtesting_engine;
using backtesting_engine_models;

namespace backtesting_engine_operations;

public static class RequestOpenTrade
{
    // TradeRequestObj (Symbol, Direction)
    public static OpenTradeObject Request(RequestObject requestObj){   
        return new OpenTradeObject(){
            openValue = requestObj.value
        };
    }

}