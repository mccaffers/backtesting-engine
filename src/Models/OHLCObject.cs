using System;
using Newtonsoft.Json;

namespace backtesting_engine;

public class OhlcObject {
    [JsonProperty("d")]
    public DateTime date {get;set;} = DateTime.MinValue;
    [JsonProperty("o")]
    public decimal open {get;set;} = Decimal.Zero;
    [JsonProperty("c")]
    public decimal close {get;set;} = Decimal.Zero;
    [JsonProperty("h")]
    public decimal high {get;set;} = Decimal.MinValue;
    [JsonProperty("l")]
    public decimal low {get;set;} = Decimal.MaxValue;
    [JsonIgnore]
    public bool complete {get;set;} = false;
    
}
