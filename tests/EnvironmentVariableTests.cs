using System;
using System.Collections.Generic;
using backtesting_engine;
using Moq;
using Moq.Protected;
using Utilities;
using Xunit;

namespace Tests;

public class EnvironmentVariableTests
{
    [Fact]
    public void CheckEnvironmentVariables()
    {
        // Arrange
        var environmentMock = new Mock<IEnvironmentVariables>();
        environmentMock.SetupGet<string[]>(x=>x.symbols).Returns(new string[]{"TestEnvironmentSetup"});

        // Assert
        Assert.Equal(new string[]{"TestEnvironmentSetup"}, environmentMock.Object.symbols);
    }

    [Fact]
    public void CheckEnvironmentVariables2()
    {
        // Arrange
        var environmentMock = new Mock<EnvironmentVariables>();

        environmentMock.SetupGet<bool>(x=>x.loadFromEnvironmnet).Returns(false);
        environmentMock.SetupGet<string>(x=>x.scalingFactor).Returns("TestEnvironmentSetup,1;");

        Dictionary<string, decimal> localdictionary = new Dictionary<string, decimal>();
        localdictionary.Add("TestEnvironmentSetup", 1m);

        var scalingFactor = environmentMock.Object.GetScalingFactor("TestEnvironmentSetup");

        // Assert
        Assert.Equal(1, scalingFactor);
        Assert.Equal(localdictionary, environmentMock.Object.scalingFactorDictionary);

    }




}