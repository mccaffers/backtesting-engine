using backtesting_engine;
using MemoryPack;

namespace backtesting_engine_models;


[MemoryPackable]
public partial class RequestObject
{

    public RequestObject(PriceObj priceObj,
                            TradeDirection direction,
                            string dealReference,
                            decimal scalingFactor,
                            decimal stopDistancePips,
                            decimal limitDistancePips,
                            decimal size,
                            string strategyId)
    {
        this.priceObj = priceObj;
        this.direction = direction;
        this.dealReference = dealReference;
        this.symbol = priceObj.symbol;
        this.scalingFactor = scalingFactor;
        this.stopDistancePips = stopDistancePips;
        this.limitDistancePips = limitDistancePips;
        this.size = size;
        this.strategyId = strategyId;

        spread = (priceObj.ask - priceObj.bid) * scalingFactor;
        date = priceObj.date;

        if (direction == TradeDirection.SELL)
        {
            level = priceObj.bid;
            stopLevel = level + (stopDistancePips / scalingFactor);
            limitLevel = level - (limitDistancePips / scalingFactor);
        }
        else if (direction == TradeDirection.BUY)
        {
            level = priceObj.ask;
            stopLevel = level - (stopDistancePips / scalingFactor);
            limitLevel = level + (limitDistancePips / scalingFactor);
        }
    }

    public PriceObj priceObj { get; }
    public string symbol { get; }
    public DateTime date { get; }
    public decimal spread { get; set; }
    public string strategyId { get; set; }

    // When setting the direction, set the current level (ASK/BID)
    public TradeDirection direction { get; }
    public decimal scalingFactor { get; }
    public decimal stopDistancePips { get; }
    public decimal limitDistancePips { get; }

    // We update this in live trading
    public string dealReference { get; set; }


    // TODO: Make these readonly and set in the constructor
    // When setting the stop/limit level, calculate based on direction and current price
    public decimal stopLevel { get; set; }
    public decimal limitLevel { get; set; }

    public decimal level { get; private set; }
    public decimal closeLevel { get; private set; }
    public decimal profit { get; private set; }
    public DateTime closeDateTime { get; private set; }

    public decimal size { get; set; }

    public void UpdateClose(PriceObj priceObj)
    {
        closeLevel = direction == TradeDirection.BUY ? priceObj.bid : priceObj.ask;
        closeDateTime = priceObj.date;
        profit = ((direction == TradeDirection.BUY ? closeLevel - level : level - closeLevel)
                    * scalingFactor)
                        * size;

    }

}