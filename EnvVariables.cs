using System.Collections.Immutable;
using Utilities;

namespace backtesting_engine
{
    public static class EnvVariables
    {
        public static readonly ImmutableArray<string> symbols = ImmutableArray.Create(EnvironmentVariable.Get("symbols").Split(","));
        public static readonly string folderPath = EnvironmentVariable.Get("folderPath");

    }
}