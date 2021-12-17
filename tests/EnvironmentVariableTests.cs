using System;
using backtesting_engine;
using Xunit;

namespace Tests;

public class EnvironmentVariableTests
{
    [Fact]
    public void CheckEnvironmentVariables()
    {
        // Arrange
        var key = "Env";
        var input = "Test";
        Environment.SetEnvironmentVariable(key, input);

        // Act
        var output = Utilities.EnvironmentVariables.Get(key);

        // Assert
        Assert.Equal(input, output);
    }

    [Fact]
    public void CheckMissingEnvVariable(){
        Assert.Throws<ArgumentException>(() => Utilities.EnvironmentVariables.Get("Missing"));
    }


}