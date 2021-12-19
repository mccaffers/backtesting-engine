namespace Utilities
{
    public class EnvironmentVariables
    {
        public virtual string Get(string envName){
            var output = Environment.GetEnvironmentVariable(envName);
            if(output == null){
                throw new ArgumentException("Missing environment variable " + envName);
            }
            return output;
        }
    }
}