namespace backtesting_engine.interfaces;

public interface ISystemObjects
{
    DateTime systemStartTime { get; }
    string systemMessage { get; set; }
    
}
