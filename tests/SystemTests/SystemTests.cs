
using System;
using System.Threading.Tasks;
using backtesting_engine;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using backtesting_engine_operations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using trading_exception;
using Utilities;
using Xunit;

namespace Tests;

public class SystemTests
{

    [Fact]
    public void TestTradeException(){

        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        var environmentObj = environmentMock.Object;

        var provider = new ServiceCollection()
            .AddSingleton<IEnvironmentVariables>(environmentObj)
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ITaskManager>(new Mock<ITaskManager>().Object)
            .AddSingleton<IElasticClient, ElasticClient>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .BuildServiceProvider();

        var reportingMock = new Mock<IReporting>();

        var taskManager = provider.GetService<ITaskManager>();
        var environmentVariables = provider.GetService<IEnvironmentVariables>();

        var systemMock = new Mock<SystemSetup>(provider, reportingMock.Object, environmentObj);
        systemMock.Setup(x=>x.StartEngine())
                        .Throws(new TradingException(message: "error", environmentObj));

        var output = "";
        reportingMock.Setup(x=>x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
        reportingMock.Setup(x=>x.EndOfRunReport(It.IsAny<string>())).Callback((string message) => {
            output=message;
        });

        var x = systemMock.Object; // Call System Constructor

        Assert.Equal("error", output);

    }
    
    [Fact]
    public void TestNormalException(){

        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        var environmentObj = environmentMock.Object;

        var provider = new ServiceCollection()
            .AddSingleton<IEnvironmentVariables>(environmentObj)
            .AddSingleton<ITaskManager>(new Mock<ITaskManager>().Object)
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<IElasticClient, ElasticClient>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .BuildServiceProvider();

        var reportingMock = new Mock<IReporting>();

        var taskManager = provider.GetService<ITaskManager>();
        var environmentVariables = provider.GetService<IEnvironmentVariables>();

        var systemMock = new Mock<SystemSetup>(provider, reportingMock.Object, environmentObj);
        systemMock.Setup(x=>x.StartEngine())
                        .Throws(new ArgumentException(message: "error"));
                        
        var output = "";
        reportingMock.Setup(x=>x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
        reportingMock.Setup(x=>x.EndOfRunReport(It.IsAny<string>())).Callback((string message) => {
            output=message;
        });

        var x = systemMock.Object; // Call System Constructor

        Assert.Equal("error", output);

    }


}