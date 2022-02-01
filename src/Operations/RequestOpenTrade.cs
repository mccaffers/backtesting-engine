using backtesting_engine;
using backtesting_engine_models;

namespace backtesting_engine_operations;

public static class RequestOpenTrade
{
    // TradeRequestObj (Symbol, Direction)
    public static OpenTradeObject Request(RequestObject requestObj){   

        decimal stopDistance = 50;
        decimal limitDistance = 50;

        decimal slippage = 1m/requestObj.scalingFactor;
        decimal level = requestObj.level;
        var pips = 0m;
        decimal stopLevel = 0m;
        decimal limitLevel = 0m;

        if(requestObj.direction == "SELL"){

            level-=slippage;

            stopLevel = level + (stopDistance/(decimal)requestObj.scalingFactor);
            limitLevel = level - (limitDistance/(decimal)requestObj.scalingFactor);

            pips = requestObj.level - limitLevel;

        } else if (requestObj.direction == "BUY"){
            
            level+=slippage;
            
            if(stopLevel==0){
                stopLevel = level - (stopDistance/requestObj.scalingFactor);
            }

            if(limitLevel==0){
                limitLevel = level + (limitDistance/requestObj.scalingFactor);
            }

            pips = limitLevel - requestObj.level;
        }

        return new OpenTradeObject(){
            openValue = requestObj.level,
            level = requestObj.level,
            limitLevel = stopLevel,
            stopLevel = limitLevel,
            direction = requestObj.direction
        };
    }

}