using backtesting_engine.interfaces;

namespace backtesting_engine;

public class SystemObjects : ISystemObjects
{
    public DateTime systemStartTime { get; } = DateTime.Now;
    public string? systemMessage { get; set; }
}