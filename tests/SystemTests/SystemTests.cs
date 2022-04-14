
using System.Threading.Tasks;
using backtesting_engine;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
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
    public async Task TestException(){
        // var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        // var environmentObj = environmentMock.Object;
        // var provider = new ServiceCollection()
        //     .AddSingleton<IEnvironmentVariables>(environmentObj)
        //     .AddSingleton<ITradingObjects, TradingObjects>()
        //     .AddSingleton<ISystemObjects, SystemObjects>()
        //     .BuildServiceProvider();

        // var reportingMock = new Mock<Reporting>(provider, new Mock<IElasticClient>().Object, environmentObj);

        // // Setup local dependency provider
        // // provider = new ServiceCollection()
        // //     .AddSingleton<IEnvironmentVariables>(environmentObj)
        // //     .AddSingleton<IElasticClient>(new Mock<IElasticClient>().Object)
        // //     .AddSingleton<IReporting>(reportingMock.Object)
        // //     .AddSingleton<IIngest, backtesting_engine_ingest.Ingest>()
        // //     .AddSingleton<IConsumer, Consumer>()
        // //     .AddSingleton<ITaskManager, TaskManager>()
        // //     .BuildServiceProvider(true);

        // var systemMock = new Mock<SystemSetup>();
        // systemMock.Setup(x=>x.StartEngine(It.IsAny<ITaskManager>(), It.IsAny<IEnvironmentVariables>()))
        //                 .Throws(new TradingException(message: "error"));

        // var output = "";
        // reportingMock.Setup(x=>x.SendStack(It.IsAny<TradingException>())).Returns(Task.FromResult(true));
        // reportingMock.Setup(x=>x.EndOfRunReport(It.IsAny<string>())).Returns((string message) => {
        //     output=message;
        // });

        // var taskManager = provider.GetService<ITaskManager>();
        // var environmentVariables = provider.GetService<IEnvironmentVariables>();

        // await systemMock.Object.StartEngine(taskManager, environmentVariables);

        // Assert.Equal("error", output);

    }

}