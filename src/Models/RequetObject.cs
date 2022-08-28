using backtesting_engine;
using backtesting_engine.interfaces;

namespace backtesting_engine_models;

public class RequestObject {
    
    private readonly IEnvironmentVariables env;

    public RequestObject(PriceObj priceObj, TradeDirection direction, IEnvironmentVariables env, string key)
    {
        this.priceObj = priceObj;
        this.direction = direction;
        this.openDate = priceObj.date;
        this.key = key;
        this.symbol = priceObj.symbol;
        this.env = env;
        this.scalingFactor = env.GetScalingFactor(priceObj.symbol);
    }

    // When setting the direction, set the current level (ASK/BID)
    private TradeDirection _direction;
    public TradeDirection direction {
        get {
            return _direction;
        }
        set {
            _direction = value;
            this.level = _direction == TradeDirection.BUY ? this.priceObj.ask : this.priceObj.bid;
        }
    }

    private readonly decimal scalingFactor;

    private decimal _stopDistancePips;
    public decimal stopDistancePips {
        get{
            return _stopDistancePips;
        }
        set {
            this._stopDistancePips = value;

            if (this.direction == TradeDirection.SELL)
            {
                this.stopLevel = this.level + (this._stopDistancePips / env.GetScalingFactor(this.symbol));

            } else if (this.direction == TradeDirection.BUY)
            {
                this.stopLevel = this.level - (this._stopDistancePips / env.GetScalingFactor(this.symbol));
            }
        }
    }

    private decimal _limitDistancePips;
    public decimal limitDistancePips { 
        get {
            return this._limitDistancePips;
        }
        set {
            this._limitDistancePips = value;

            if (this.direction == TradeDirection.SELL)
            {
                this.limitLevel = this.level - (this.limitDistancePips / env.GetScalingFactor(this.symbol));

            }
            else if (this.direction == TradeDirection.BUY)
            {
                this.limitLevel = this.level + (this.limitDistancePips / env.GetScalingFactor(this.symbol));
            }
        }   
    }

    public decimal stopLevel { get; set; }
    public decimal limitLevel { get; set; }
    
    // Read only properties, defined in the constructor
    public string key {get;}
    public PriceObj priceObj { get; }
    public DateTime openDate {get;}
    public string symbol {get;}

    // Private set propertises
    public decimal level {get; private set;}
    public decimal closeLevel {get; private set;}
    public decimal profit {get; private set;}
    public DateTime closeDateTime {get; private set;}

    // Can only be set in the initialisation of the object
    public decimal size {get;set;}


    public void UpdateClose(PriceObj priceObj){

        this.closeLevel = this.direction == TradeDirection.BUY ? priceObj.bid : priceObj.ask;
        this.closeDateTime = priceObj.date;
        this.profit = ((this.direction == TradeDirection.BUY ? this.closeLevel - this.level : this.level - this.closeLevel) 
                        * this.scalingFactor)
                            * this.size;

    }

    public void UpdateLevelWithSlippage(decimal slippage){
        this.level = this.direction == TradeDirection.BUY ? this.level-slippage : this.level+slippage;
    }
}
