namespace backtesting_engine;
using MemoryPack;

[MemoryPackable]
public partial class OhlcObject {

    public DateTime date {get;set;} = DateTime.MinValue;

    public decimal open {get;set;} = Decimal.Zero;

    public decimal close {get;set;} = Decimal.Zero;

    public decimal high {get;set;} = Decimal.MinValue;

    public decimal low {get;set;} = Decimal.MaxValue;
    
    [MemoryPackIgnore]
    public bool complete {get;set;} = false;
    
}
