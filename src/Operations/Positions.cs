using backtesting_engine;
using backtesting_engine_ingest;
using backtesting_engine_models;
using trading_exception;
using Utilities;

namespace backtesting_engine_operations;

public interface IPositions
{
    void CloseAll();
    IEnumerable<RequestObject> GetOrderBook(string symbol);
    void Review(PriceObj priceObj);
    void KinetStopLimit(PriceObj priceObj);
    void UpdateTradeHistory(RequestObject reqObj);
    void ReviewEquity();
}

public class Positions : TradingBase, IPositions
{
    readonly ICloseOrder closeOrder;
    readonly IEnvironmentVariables envVaribles;
    public Positions(IServiceProvider provider, ICloseOrder closeOrder, IEnvironmentVariables envVaribles) : base(provider)
    {
        this.closeOrder = closeOrder;
        this.envVaribles = envVaribles;
    }

    public void KinetStopLimit(PriceObj priceObj)
    {
        if(envVaribles.kineticStopLoss == 0){
            return;
        }

        foreach (var myTradeObj in GetOrderBook(priceObj.symbol))
        {
            myTradeObj.UpdateClose(priceObj);

            var distance = envVaribles.kineticStopLoss;

            // if(myTradeObj.profit>50){
            //     distance=distance/2;      
            // }

            // if(myTradeObj.profit>100){
            //     distance=distance/3;      
            // }

            if(myTradeObj.direction == TradeDirection.BUY){
                var proposedStoplevel = myTradeObj.close - (distance / envVaribles.GetScalingFactor(priceObj.symbol));
                if(proposedStoplevel > myTradeObj.stopLevel){
                    myTradeObj.stopLevel=proposedStoplevel;
                }

                if(envVaribles.kineticLimit !=0){
                    var proposedLimit = myTradeObj.close + (distance / envVaribles.GetScalingFactor(priceObj.symbol));
                    if(proposedLimit > myTradeObj.limitLevel){
                        myTradeObj.limitLevel = proposedLimit;
                    }
                }
                
            } else if(myTradeObj.direction == TradeDirection.SELL){
                var proposedStoplevel = myTradeObj.close + (distance / envVaribles.GetScalingFactor(priceObj.symbol));
                if(proposedStoplevel < myTradeObj.stopLevel){
                    myTradeObj.stopLevel=proposedStoplevel;
                }

                if(envVaribles.kineticLimit !=0){
                    var proposedLimit = myTradeObj.close - (distance / envVaribles.GetScalingFactor(priceObj.symbol));
                    if(proposedLimit < myTradeObj.limitLevel){
                        myTradeObj.limitLevel = proposedLimit;
                    }
                }
            }
        }

    }

    public void ReviewEquity()
    {
        
        if (this.tradingObjects.accountObj.hasAccountExceededDrawdownThreshold()){
            // close all trades
            CloseAll();

            // stop any more trades
            throw new TradingException("Exceeded threshold PL:" + this.tradingObjects.accountObj.pnl, "", envVaribles);
        }
    }

    public void CloseAll()
    {
        foreach (var item in this.tradingObjects.openTrades)
        {
            UpdateTradeHistory(item.Value);
        }
    }

    public void Review(PriceObj priceObj)
    {

        this.tradingObjects.tradeTime = priceObj.date;

        foreach (var myTradeObj in GetOrderBook(priceObj.symbol))
        {

            myTradeObj.UpdateClose(priceObj);

            if (myTradeObj.direction == TradeDirection.BUY &&
                    (priceObj.bid <= myTradeObj.stopLevel || priceObj.bid >= myTradeObj.limitLevel))
            {
                UpdateTradeHistory(myTradeObj);
            }
            else if (myTradeObj.direction == TradeDirection.SELL &&
                   (priceObj.ask >= myTradeObj.stopLevel || priceObj.ask <= myTradeObj.limitLevel))
            {
                UpdateTradeHistory(myTradeObj);
            }
        }
    }

    public IEnumerable<RequestObject> GetOrderBook(string symbol)
    {
        return this.tradingObjects.openTrades.Where(x => x.Key.Contains(symbol)).Select(x => x.Value);
    }

    public void UpdateTradeHistory(RequestObject reqObj)
    {

        TradeHistoryObject tradeHistoryObj = new TradeHistoryObject();
        tradeHistoryObj.closeLevel = reqObj.close;
        tradeHistoryObj.profit = reqObj.profit;
        tradeHistoryObj.closeDateTime = reqObj.closeDate;
        tradeHistoryObj.runningTime = reqObj.closeDate.Subtract(reqObj.openDate).TotalMinutes;

        PropertyCopier<RequestObject, TradeHistoryObject>.Copy(reqObj, tradeHistoryObj);
        this.closeOrder.Request(tradeHistoryObj);
    }



}
