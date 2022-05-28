using backtesting_engine.analysis;
using Utilities;

namespace backtesting_engine.interfaces;

public interface ISystemSetup
{
    Task<string> SendStackException(Exception ex);
    Task<string> StartEngine();
}
