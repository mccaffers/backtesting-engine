using System;

namespace Utilities
{
    public static class EnvironmentVariable
    {
        public static string Get(string envName){

            var output = Environment.GetEnvironmentVariable(envName);

            if(output == null){
                System.Console.WriteLine("Doesn't contain env " + envName);
                throw new ArgumentException("Missing environment variable " + envName);
            }

            return output;
        }
    }
}