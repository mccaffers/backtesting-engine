using backtesting_engine;
using backtesting_engine.interfaces;
using Utilities;

namespace backtesting_engine_web;

public interface IWebUtils
{
    Task Invoke(PriceObj priceObj);
}

public class WebUtils : IWebUtils
{
    private List<OhlcObject> ohlcList = new List<OhlcObject>();
    private OhlcObject lastItem = new OhlcObject();
    private readonly IWebNotification webNotification;

    public WebUtils(IWebNotification webNotification)
    {
        this.webNotification = webNotification;
    }

    public async Task Invoke(PriceObj priceObj)
    {
        ohlcList = GenericOhlc.CalculateOHLC(priceObj, priceObj.ask, TimeSpan.FromMinutes(60), ohlcList);

        if (ohlcList.Count > 1)
        {
            var secondLast = ohlcList[ohlcList.Count - 2];
            if (secondLast.complete && secondLast.close != lastItem.close)
            {
                await webNotification.PriceUpdate(secondLast, true);
                lastItem = secondLast;
            }
            else
            {
                await webNotification.PriceUpdate(ohlcList.Last());
            }
        }

    }
}
