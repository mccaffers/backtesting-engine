namespace backtesting_engine;

public interface ISystemSetup
{
    Task StartEngine(ITaskManager main);
}