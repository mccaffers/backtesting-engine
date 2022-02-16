using backtesting_engine;
using backtesting_engine_models;

namespace backtesting_engine_operations;

public static class RequestOpenTrade
{
    // TradeRequestObj (Symbol, Direction)
    public static OpenTradeObject Request(RequestObject requestObj){   

        decimal stopDistance = 50/requestObj.scalingFactor;
        decimal limitDistance = 50/requestObj.scalingFactor;

        decimal slippage = 1m/requestObj.scalingFactor;
        decimal level = requestObj.level;
        decimal stopLevel = 0m;
        decimal limitLevel = 0m;

        if(requestObj.direction == TradeDirection.SELL){

            level-=slippage;

            stopLevel = level + stopDistance;
            limitLevel = level - limitDistance;
            
        } else if (requestObj.direction == TradeDirection.BUY){
            
            level+=slippage;
            
            stopLevel = level - stopDistance;
            limitLevel = level + limitDistance;
        }

        return new OpenTradeObject(){
            level = level,
            limitLevel = limitLevel,
            stopLevel = stopLevel,
            direction = requestObj.direction,
            scalingFactor = requestObj.scalingFactor,
            size = requestObj.size
        };
    }

}