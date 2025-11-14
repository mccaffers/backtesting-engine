using MemoryPack;

namespace backtesting_engine;

[MemoryPackable(GenerateType.Object)]
public partial class PriceObj {
    public string symbol { get;set; } = "";
    public decimal bid {get;set;}
    public decimal ask {get;set;}
    public DateTime date {get; set;}
}
