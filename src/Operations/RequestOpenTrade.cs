using backtesting_engine;
using backtesting_engine_models;

namespace backtesting_engine_operations;

public static class RequestOpenTrade
{
    // Validation on opening a trade
    public static void Request(RequestObject reqObj){   

        decimal stopLevel = 0m;
        decimal limitLevel = 0m;

        decimal slippage = 1m/reqObj.priceObj.scalingFactor;
        reqObj.UpdateLevelWithSlippage(slippage);

        if(reqObj.stopDistancePips != 0 && reqObj.limitDistancePips!=0){

             if(reqObj.direction == TradeDirection.SELL) {
                stopLevel = reqObj.level +  (reqObj.stopDistancePips / reqObj.priceObj.scalingFactor);
                limitLevel = reqObj.level - (reqObj.limitDistancePips / reqObj.priceObj.scalingFactor);
                
            } else if (reqObj.direction == TradeDirection.BUY) {
                stopLevel = reqObj.level - (reqObj.stopDistancePips / reqObj.priceObj.scalingFactor);
                limitLevel = reqObj.level + (reqObj.limitDistancePips / reqObj.priceObj.scalingFactor);
            }
        }

        reqObj.stopLevel = stopLevel;
        reqObj.limitLevel = limitLevel;

        OpenTrade.Request(reqObj);
    }

}