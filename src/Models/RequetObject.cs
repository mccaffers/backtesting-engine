using backtesting_engine;
using Utilities;

namespace backtesting_engine_models;

public class RequestObject {
    
    private readonly IEnvironmentVariables env;

    public RequestObject(PriceObj priceObj, TradeDirection direction, IEnvironmentVariables env)
    {
        this.priceObj = priceObj;
        this.direction = direction;
        this.openDate = priceObj.date;
        this.key = DictionaryKeyStrings.OpenTrade(priceObj);
        this.symbol = priceObj.symbol;
        this.env = env;

        UpdateLevelWithSlippage(1m / env.GetScalingFactor(this.symbol));
    }

    // When setting the direction, set the current level (ASK/BID)
    private TradeDirection _direction;
    public TradeDirection direction {
        get {
            return _direction;
        }
        init {
            _direction = value;
            this.level = _direction == TradeDirection.BUY ? this.priceObj.ask : this.priceObj.bid;
        }
    }

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
    public decimal close {get; private set;}
    public decimal profit {get; private set;}
    public DateTime closeDate {get; private set;}

    // Can only be set in the initialisation of the object
    public decimal size {get;init;}


    public void UpdateClose(PriceObj priceObj, decimal scalingFactor){

        this.close = this.direction == TradeDirection.BUY? priceObj.bid : priceObj.ask;
        this.closeDate = priceObj.date;
        this.profit = ((this.direction == TradeDirection.BUY ? this.close - this.level : this.level - this.close) 
                        * scalingFactor)
                            * this.size;
    }

    public void UpdateLevelWithSlippage(decimal slippage){
        this.level = this.direction == TradeDirection.SELL? this.level+slippage : this.level-slippage;
    }
}
