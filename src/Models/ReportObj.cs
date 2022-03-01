namespace backtesting_engine;

public class ReportObj {

    public string[]? symbols { get;set; }
    public decimal pnl {get;set;}
    public string? runID {get;set;}
    public decimal openingEquity { get;set; }
    public decimal maximumDrawndownPercentage {get;set;}
    public string? strategy {get;set;}
}
