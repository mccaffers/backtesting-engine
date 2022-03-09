using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_ingest;
using Moq;
using Utilities;
using Xunit;

namespace Tests;

[Collection("Sequential")]
public class DataInputTests
{


    [Theory]
    [InlineData(1, "2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75")]
    [InlineData(0, "UTC,AskPrice,BidPrice,AskVolume,BidVolume")]
    [InlineData(0, "2018-01-01T01:00:00.594+00:00,,,,")]
    [InlineData(0, ",,,,")]
    [InlineData(0, "")]
    public void TestPopulateLocalBuffer(int expectedResult, string line){

        TestEnvironment.SetEnvironmentVariables(); 

        var inputMock = new Mock<Ingest>(){
            CallBase = true
        };

        MethodInfo? populateLocalBuffer = inputMock?.Object.GetType().GetMethod("PopulateLocalBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
        var fileName = "TestEnvironmentSetup";

        populateLocalBuffer?.Invoke(inputMock?.Object, new object[]{fileName, line});

        Assert.Equal(expectedResult, inputMock?.Object.localInputBuffer.Count);
    }
    
    [Fact]
    public async void TestingReadFile()
    {

        TestEnvironment.SetEnvironmentVariables(); 
       
        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(){
            CallBase = true
        };

        inputMock.Setup(x=>x.EnvironmentSetup());

        // Ingore the consumer
        consumerMock.Setup<Task>(x=>x.ConsumeAsync(It.IsAny<BufferBlock<PriceObj>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
        // programMock.Setup(x=>x.EnvironmentSetup());
        // inputMock.Setup(x=>x.ReadLines(It.IsAny<BufferBlock<PriceObj>>())).Returns(Task.CompletedTask);
//
        var programObj = programMock.Object;
        inputMock.Object.fileNames.Add(Path.Combine(PathUtil.GetTestPath("TestEnvironmentSetup"), "testSymbol.csv"));

        await programObj.IngestAndConsume(consumerMock.Object, inputMock.Object);

        BufferBlock<PriceObj> buffer = programObj.GetFieldValue<BufferBlock<PriceObj>>("buffer");
        IList<PriceObj>? items;
        var output = buffer.TryReceiveAll(out items);

        Assert.True(items!=null && items.Count == 1);
    }
}