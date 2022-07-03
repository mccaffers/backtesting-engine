namespace backtesting_engine_models;

public class VolatilityObject {
    public DateTime date {get;set;}
    public string[] symbols {get;set;} = Array.Empty<string>();
    public string runID {get;set;} = string.Empty;
    public int runIteration {get;set;}
    public string strategy {get;set;} = string.Empty;
    public decimal dayClose { get;set;}
    public decimal dayRange { get;set;}
    public decimal totalMovement { get;set;}
    public decimal distanceBetweenPriceMoves { get;set;}
    public decimal dayCloseGap { get;set;}
    public decimal spreadDistance {get;set;}
}