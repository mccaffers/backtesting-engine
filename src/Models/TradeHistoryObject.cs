namespace backtesting_engine_models;

public class TradeHistoryObject {
    public string? symbol {get;set;}
    public string? direction { get;set;}
    public decimal openValue {get;set;}
    public decimal size {get;set;}
    public decimal profit {get;set;}
    public decimal level {get;set;}
    public decimal scalingFactor {get;set;}
    public decimal closeLevel {get;set;}
    public DateTime openDate {get;set;}
    public DateTime currentDateTime {get;set;}
    public double runningTime {get;set;}
    public decimal stopLevel {get;set;}
    public decimal limitLevel {get;set;}
}
