
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

[Collection("Sequential")]
public class SystemTests
{

    // [Fact]
    // public void TestTradeException()
    // {

    //     // Arrange
    //     TestEnvironment.SetEnvironmentVariables(); 

    //     var provider = new ServiceCollection()
    //         .AddSingleton<ITradingObjects, TradingObjects>()
    //         .AddSingleton<ITaskManager>(new Mock<ITaskManager>().Object)
    //         .AddSingleton<IElasticClient, ElasticClient>()
    //         .AddSingleton<ISystemObjects, SystemObjects>()
    //         .BuildServiceProvider();

    //     var reportingMock = new Mock<IReporting>();
    //     var esMock = new Mock<IElasticClient>().Object;

    //     IPositions position = new Mock<IPositions>().Object;
    //     var systemMock = new Mock<SystemSetup>(provider, reportingMock.Object, esMock, position);

    //     systemMock.Setup(x => x.StartEngine())
    //                     .Throws(new TradingException(message: "error", ""));

    //     var output = "";
    //     reportingMock.Setup(x => x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
    //     reportingMock.Setup(x => x.EndOfRunReport(It.IsAny<string>())).Callback((string message) =>
    //     {
    //         output = message;
    //     });

    //     var x = systemMock.Object; // Call System Constructor

    //     Assert.Equal("error", output);

    //     // Clean
    //     TestEnvironment.CleanEnvironment();
    // }

    // [Fact]
    // public void TestNormalException()
    // {

    //     // Arrange
    //     TestEnvironment.SetEnvironmentVariables(); 

    //     // Setup the Service Provider just for the services we need
    //     var provider = new ServiceCollection()
    //         .AddSingleton<ITaskManager>(new Mock<ITaskManager>().Object)
    //         .AddSingleton<ITradingObjects, TradingObjects>()
    //         .AddSingleton<ISystemObjects, SystemObjects>()
    //         .AddSingleton<IElasticClient>(new Mock<IElasticClient>().Object)

    //         .BuildServiceProvider();


    //     // Mock an empty reporting object, we don't want to send any reports
    //     var reportingMock = new Mock<IReporting>();
    //     var esMock = new Mock<IElasticClient>().Object;

    //     IPositions position = new Mock<IPositions>().Object;
    //     var systemMock = new Mock<SystemSetup>(provider, reportingMock.Object, esMock, position);
    //     systemMock.Setup(x => x.StartEngine())
    //                     .Throws(new ArgumentException(message: "error"));

    //     var output = "";
    //     reportingMock.Setup(x => x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
    //     reportingMock.Setup(x => x.EndOfRunReport(It.IsAny<string>())).Callback((string message) =>
    //     {
    //         output = message;
    //     });

    //     var x = systemMock.Object; // Call System Constructor

    //     Assert.Equal("error", output);

    //     // Clean
    //     TestEnvironment.CleanEnvironment();

    // }


}