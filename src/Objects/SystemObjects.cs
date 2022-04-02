using System.Collections.Concurrent;
using backtesting_engine;
using backtesting_engine_models;

public interface ISystemObjects
{
    DateTime systemStartTime { get; }
}

public class SystemObjects : ISystemObjects
{

    public DateTime systemStartTime { get; } = DateTime.Now;

    public static string systemMessage { get; set; } = "";


}