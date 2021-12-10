using Utilities;

namespace backtesting_engine
{
    public class EnvVariables
    {
        public static readonly string[] symbols = EnvironmentVariable.Get("symbols").Split(",");
        public static readonly string folderPath = EnvironmentVariable.Get("folderPath");

    }
}