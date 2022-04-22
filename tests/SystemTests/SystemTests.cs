
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
            .AddSingleton<IElasticClient, ElasticClient>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .BuildServiceProvider();

        var reportingMock = new Mock<Reporting>(provider, new Mock<IElasticClient>().Object, environmentObj);

        var taskManager = provider.GetService<ITaskManager>();
        var environmentVariables = provider.GetService<IEnvironmentVariables>();

        var systemMock = new Mock<SystemSetup>(new Mock<ITaskManager>().Object, reportingMock.Object, environmentObj);
        systemMock.Setup(x=>x.StartEngine(It.IsAny<ITaskManager>(), It.IsAny<IEnvironmentVariables>()))
                        .Throws(new TradingException(message: "error"));

        var output = "";
        reportingMock.Setup(x=>x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
        reportingMock.Setup(x=>x.EndOfRunReport(It.IsAny<string>())).Returns((string message) => {
            output=message;
            return Task.FromResult(message);
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
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<IElasticClient, ElasticClient>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .BuildServiceProvider();

        var reportingMock = new Mock<Reporting>(provider, new Mock<IElasticClient>().Object, environmentObj);

        var taskManager = provider.GetService<ITaskManager>();
        var environmentVariables = provider.GetService<IEnvironmentVariables>();

        var systemMock = new Mock<SystemSetup>(new Mock<ITaskManager>().Object, reportingMock.Object, environmentObj);
        systemMock.Setup(x=>x.StartEngine(It.IsAny<ITaskManager>(), It.IsAny<IEnvironmentVariables>()))
                        .Throws(new ArgumentException(message: "error"));
                        
        var output = "";
        reportingMock.Setup(x=>x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
        reportingMock.Setup(x=>x.EndOfRunReport(It.IsAny<string>())).Returns((string message) => {
            output=message;
            return Task.FromResult(message);
        });

        var x = systemMock.Object; // Call System Constructor

        Assert.Equal("error", output);

    }


}