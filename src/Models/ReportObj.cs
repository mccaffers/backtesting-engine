namespace backtesting_engine;

public class ReportFinalObj {

    // System information
    public DateTime date {get;set;}
    public string? runID {get;set;}
    public string? hostname {get;set;}
    public double systemRunTimeInMinutes {get;set;}
    public int runIteration {get;set;}

    // Engine Information
    public string[]? symbols { get;set; }
    public string? strategy {get;set;}
    public bool complete {get;set;} = true;
    public string? reason {get;set;}
    public string? detailedReason {get;set;}

    // Account stats
    public decimal openingEquity { get;set; }
    public decimal maximumDrawndownPercentage {get;set;}

    // Trading Stats
    public decimal pnl {get;set;}
    public double tradingTimespanInDays {get;set;}
    public int positiveTradeCount {get;set;}
    public int negativeTradeCount {get;set;}
    public int positivePercentage {get;set;}


    
}

public class ReportTradeObj {
    public DateTime date {get;set;}
    public string[]? symbols { get;set; }
    public decimal pnl {get;set;}
    public string? runID {get;set;}
    public decimal tradeProfit {get;set;}
    public int runIteration {get;set;}
}
