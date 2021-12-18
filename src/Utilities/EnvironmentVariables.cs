using System.Collections.Immutable;

namespace Utilities
{
    public static class EnvironmentVariables
    {

        public static readonly ImmutableArray<string> symbols = ImmutableArray.Create(EnvironmentVariables.Get("symbols").Split(","));
        public static readonly string folderPath = EnvironmentVariables.Get("folderPath");

        public static string Get(string envName){
            var output = Environment.GetEnvironmentVariable(envName);
            return output ?? throw new ArgumentException("Missing environment variable " + envName);
        }
    }
}