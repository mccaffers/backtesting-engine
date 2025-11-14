using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using trading_exception;
using Utilities;

namespace backtesting_engine_operations;

public interface IPositions
{
    void CloseAll();
    IEnumerable<RequestObject> GetOrderBook(string symbol);
    Task Review(PriceObj priceObj);
    void TrailingStopLoss(PriceObj priceObj);
    void UpdateTradeHistory(RequestObject reqObj, PriceObj priceObj);
    void ReviewEquity(PriceObj priceObj);
    void ReviewMonthlyPnL(PriceObj priceObj);
}

public class Positions : TradingBase, IPositions
{
    readonly ICloseOrder closeOrder;
    readonly IOpenOrder openOrder;

    public Positions(IServiceProvider provider, IOpenOrder openOrder, ICloseOrder closeOrder) : base(provider)
    {
        this.closeOrder = closeOrder;
        this.openOrder = openOrder;
    }

    public void TrailingStopLoss(PriceObj priceObj)
    {
        // Check Trailing Stop Loss is active
        if (TradingVariables.TRAILING_STOP_LOSS_ACTIVE.Value() == "0")
        {
            return;
        }

        foreach (var myTradeObj in GetOrderBook(priceObj.symbol))
        {
            // Update the close price, referenced in further conditions
            myTradeObj.UpdateClose(priceObj);

            var distance = decimal.Parse(TradingVariables.TRAILING_STOP_LOSS_VALUE.Value()) / myTradeObj.scalingFactor;

            if (myTradeObj.direction == TradeDirection.BUY)
            {
                UpdateSLForBUYDirection(myTradeObj, distance);
            }
            else if (myTradeObj.direction == TradeDirection.SELL)
            {
                UpdateSLForSELLDirection(myTradeObj, distance);
            }
        }
    }

    private void UpdateSLForBUYDirection(RequestObject? myTradeObj, decimal distance)
    {

        if (myTradeObj == null)
        {
            return;
        }

        var proposedStoplevel = myTradeObj.closeLevel - distance;
        if (proposedStoplevel > myTradeObj.stopLevel)
        {
            myTradeObj.stopLevel = proposedStoplevel;
        }

        if (decimal.Parse(TradingVariables.MOVING_LIMIT_VALUE.Value()) != 0)
        {
            var proposedLimit = myTradeObj.closeLevel + distance;
            if (proposedLimit > myTradeObj.limitLevel)
            {
                myTradeObj.limitLevel = proposedLimit;
            }
        }

    }

    private void UpdateSLForSELLDirection(RequestObject? myTradeObj, decimal distance)
    {
        if (myTradeObj == null)
        {
            return;
        }

        var proposedStoplevel = myTradeObj.closeLevel + distance;
        if (proposedStoplevel < myTradeObj.stopLevel)
        {
            myTradeObj.stopLevel = proposedStoplevel;
        }

        if (decimal.Parse(TradingVariables.MOVING_LIMIT_VALUE.Value()) != 0)
        {
            var proposedLimit = myTradeObj.closeLevel - distance;
            if (proposedLimit < myTradeObj.limitLevel)
            {
                myTradeObj.limitLevel = proposedLimit;
            }
        }
    }


    public void ReviewMonthlyPnL(PriceObj priceObj)
    {
        if (tradingObjects.openTrades.Count == 0)
        {
            return;
        }

        var currentMonth = new DateTime(priceObj.date.Date.Year, priceObj.date.Date.Month, 1, 0, 0, 0);

        if (tradingObjects.accountObj.monthlyAccountPnL.Count > 0)
        {
            var lastRecordedDateTime = tradingObjects.accountObj.monthlyAccountPnL.Last();
            var lastRecordedDate = lastRecordedDateTime.Key.Date;

            if (currentMonth.Subtract(lastRecordedDate).TotalDays > 0)
            {
                tradingObjects.accountObj.monthlyAccountPnL.TryAdd(currentMonth, tradingObjects.accountObj.pnl);
            }

            var monthlyAccount = lastRecordedDateTime.Value;
            // var monthlySum = this.tradingObjects.tradeHistory.Where(x => x.Value.closeDateTime.Month == currentMonth.Month).Sum(x => x.Value.profit);
            var percentageOfAccountLoss = (1 - (tradingObjects.accountObj.pnl / monthlyAccount)) * 100;

            if (percentageOfAccountLoss > int.Parse(BACKTESTING.MAX_MONTHLY_DRAWDOWN_THRESHOLD.Value()))
            {

                // close all trades
                CloseAll();

                // stop any more trades
                throw new TradingException("Exceeded MAX_MONTHLY_DRAWDOWN_THRESHOLD (" + BACKTESTING.MAX_MONTHLY_DRAWDOWN_THRESHOLD.Value() + ") " + this.tradingObjects.accountObj.pnl, "");
            }
        }
        else
        {
            var currentAccount = tradingObjects.accountObj.pnl;
            tradingObjects.accountObj.monthlyAccountPnL.TryAdd(currentMonth, currentAccount);
        }

    }

    public void ReviewEquity(PriceObj priceObj)
    {

        if (tradingObjects.openTrades.Count == 0)
        {
            return;
        }

        if (tradingObjects.accountObj.pnl > tradingObjects.accountObj.accountMax)
        {
            tradingObjects.accountObj.accountMax = tradingObjects.accountObj.pnl;
        }

        var localPercentDrop = (1 - (tradingObjects.accountObj.pnl / tradingObjects.accountObj.accountMax)) * 100;

        if (localPercentDrop > this.tradingObjects.accountObj.accountPercentDrop)
        {
            tradingObjects.accountObj.accountPercentDrop = localPercentDrop;
        }

        if (this.tradingObjects.accountObj.accountPercentDrop > int.Parse(BACKTESTING.MAX_OVERALL_DRAWDOWN_THRESHOLD.Value())
            && !bool.Parse(BACKTESTING.REPORT_LOSSES.Value()))
        {

            CloseAll();

            throw new TradingException(
                $"Exceeded MAX_OVERALL_DRAWDOWN_THRESHOLD ({BACKTESTING.MAX_OVERALL_DRAWDOWN_THRESHOLD.Value()}) now {tradingObjects.accountObj.pnl:F2} with a {this.tradingObjects.accountObj.maximumDrawndownPercentage:F2} from {this.tradingObjects.accountObj.accountMax:F2}%", "");
        }

    }

    public void CloseAll()
    {
        foreach (var item in this.tradingObjects.openTrades)
        {
            UpdateTradeHistory(item.Value, tradingObjects.lastPrice!);
        }
    }

    public async Task Review(PriceObj priceObj)
    {

        tradingObjects.lastPrice = priceObj;

        foreach (var myTradeObj in GetOrderBook(priceObj.symbol))
        {
            myTradeObj.UpdateClose(priceObj);

            if (myTradeObj.direction == TradeDirection.BUY &&
                    (priceObj.bid <= myTradeObj.stopLevel || priceObj.bid >= myTradeObj.limitLevel))
            {
                UpdateTradeHistory(myTradeObj, priceObj);
            }
            else if (myTradeObj.direction == TradeDirection.SELL &&
                   (priceObj.ask >= myTradeObj.stopLevel || priceObj.ask <= myTradeObj.limitLevel))
            {
                UpdateTradeHistory(myTradeObj, priceObj);
            }
        }

        await Task.CompletedTask;
    }

    public IEnumerable<RequestObject> GetOrderBook(string symbol)
    {
        if (tradingObjects.openTrades.Count > 0)
        {
            return tradingObjects.openTrades.Select(x => x.Value);
        }
        else
        {
            return [];
        }
    }

    public void UpdateTradeHistory(RequestObject reqObj, PriceObj priceObj)
    {
        closeOrder.Request(reqObj, priceObj);
    }

}
