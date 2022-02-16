using backtesting_engine;

namespace backtesting_engine_models;

public class RequestObject {
    public RequestObject(PriceObj priceObj)
    {
        this.PriceObj = priceObj;
    }

    public string? symbol {get;set;}
    private TradeDirection _direction;
    public TradeDirection direction {
        get
        {
            return _direction;
        }
        set
        {
            _direction = value;
            this.level = _direction == TradeDirection.BUY ? this.PriceObj.ask : this.PriceObj.bid;
        }
    }
    public decimal size {get;set;}
    public decimal level {get; private set;}
    public decimal scalingFactor {get;set;}
    public decimal stopLevel {get;set;}
    public decimal limitLevel {get; set;}
    public PriceObj PriceObj { get; }
}
