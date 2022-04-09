using Utilities;

namespace backtesting_engine.interfaces;

public interface ISystemSetup
{
    Task StartEngine(ITaskManager main, IEnvironmentVariables envVariables);

}