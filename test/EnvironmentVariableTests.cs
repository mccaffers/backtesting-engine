using backtesting_engine;
using Xunit;

namespace Tests;

public class EnvironmentVariableTests
{
    [Fact]
    public void CheckEnvironmentVariables()
    {
        var key = "Env";
        var input = "Test";
        
        Environment.SetEnvironmentVariable(key, input);
        var output = Utilities.EnvironmentVariables.Get(key);

        Assert.Equal(input, output);
    }

    [Fact]
    public void CheckMissingEnvVariable(){
        Assert.Throws<ArgumentException>(() => Utilities.EnvironmentVariables.Get("Missing"));
    }


}