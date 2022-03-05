namespace backtesting_engine;

public class ReportObj {
    public DateTime date {get;set;}
    public string[]? symbols { get;set; }
    public decimal pnl {get;set;}
    public string? runID {get;set;}
    public decimal openingEquity { get;set; }
    public decimal maximumDrawndownPercentage {get;set;}
    public string? strategy {get;set;}
    public string? status {get;set;}
    public string? reason {get;set;}
}
