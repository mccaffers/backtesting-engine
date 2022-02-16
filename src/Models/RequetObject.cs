namespace backtesting_engine_models;

public class RequestObject {
    public string? symbol {get;set;}
    public TradeDirection direction {get;set;}
    public decimal size {get;set;}
    public decimal level {get;set;}
    public decimal scalingFactor {get;set;}
}
