namespace backtesting_engine;

public class ReportFinalObj {

    // System information
    public DateTime date {get;set;}
    public string? runID {get;set;}
    public string? hostname {get;set;}
    public double systemRunTimeInMinutes {get;set;}
    public int runIteration {get;set;}
    public string? systemMessage {get;set;}

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
    public decimal totalProfit {get;set;}
    public decimal totalLoss {get;set;}
    public double tradingTimespanInDays {get;set;}
    public int positiveTradeCount {get;set;}
    public decimal tradingSize {get;set;}

    public int yearStart {get;set;}
    public int yearEnd {get;set;}

    public int negativeTradeCount {get;set;}
    public decimal positivePercentage {get;set;}
    public bool yearOnYearReturn {get;set;}

    public decimal stopDistanceInPips {get;set;}
    public decimal limitDistanceInPips {get;set;}

    public int instanceCount { get;set; }

    public decimal? variableA {get;set;}
    public decimal? variableB {get;set;}
    public decimal? variableC {get;set;} 
    public decimal? variableD {get;set;} 
    public decimal? variableE {get;set;} 

}

public class ReportTradeObj {
    public string id {get;} =  Guid.NewGuid().ToString();
    public DateTime date {get;set;}
    public string[]? SYMBOLS { get;set; }
    public decimal PNL {get;set;}
    public string? RUN_ID {get;set;}
    public string? STRATEGY {get;set;}
    public decimal PROFIT {get;set;}
    public int RUN_ITERATION {get;set;}
    public decimal STOP_DISTANCE_IN_PIPS {get;set;}
    public decimal LIMIT_DISTANCE_IN_PIPS {get;set;}
    public decimal TRAILING_STOP_LOSS_VALUE {get;set;}
    public int INSTANCE_COUNT { get; init; } = 0;
    public decimal SPREAD {get;set;}
    public double? TRADE_DURATION {get;set;}
}
