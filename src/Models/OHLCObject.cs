using System;

namespace backtesting_engine;

public class OhlcObject {
    public DateTime date {get;set;} = DateTime.MinValue;
    public decimal open {get;set;} = Decimal.Zero;
    public decimal close {get;set;} = Decimal.Zero;
    public decimal high {get;set;} = Decimal.MinValue;
    public decimal low {get;set;} = Decimal.MaxValue;
    public double timestamp {get;set;}
    public bool complete {get;set;} = false;
    
}
