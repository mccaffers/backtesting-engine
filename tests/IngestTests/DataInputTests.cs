using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using Moq;
using Newtonsoft.Json;
using Utilities;
using Xunit;

namespace Tests;

public class DataInputTests
{

    [Theory]
    [InlineData(1, "2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75")]
    [InlineData(0, "UTC,AskPrice,BidPrice,AskVolume,BidVolume")]
    [InlineData(0, "2018-01-01T01:00:00.594+00:00,,,,")]
    [InlineData(0, ",,,,")]
    [InlineData(0, "")]
    public void TestPopulateLocalBuffer(int expectedResult, string line){

        var envMock = TestEnvironment.SetEnvironmentVariables(); 
        var inputMock = new Mock<Ingest>(envMock.Object){
            CallBase = true
        };
        var fileName = "TestEnvironmentSetup";

        inputMock?.Object.PopulateLocalBuffer(fileName, line);

        Assert.Equal(expectedResult, inputMock?.Object.localInputBuffer.Count);
    }

    public static IEnumerable<object[]> Data =>
    new List<object[]>
    {
        // Test 1
        new object[] { 
            // Two events
            new List<string[]>() {
                new string[] { "Symbol1", "2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75"},
                new string[] { "Symbol2", "2018-01-01T02:00:00.594+00:00,1.35124,1.35065,1.5,0.75"}
            },
            "Symbol1", //  Symbol 1 is the oldest
        },

        // Test 2
        new object[] { 
            // Two events
            new List<string[]>() {
                new string[] { "Symbol1", "2018-01-01T02:00:00.594+00:00,1.35104,1.35065,1.5,0.75"},
                new string[] { "Symbol2", "2018-01-01T01:00:00.594+00:00,1.35124,1.35065,1.5,0.75"}
            },
            "Symbol2" // Symbol 2 is the oldest
        },

        // Test 3
        new object[] { 
            // Three events, 
            new List<string[]>() {
                new string[] { "Symbol1", "2018-01-01T02:00:00.594+00:00,1.35104,1.35065,1.5,0.75"},
                new string[] { "Symbol2", "2018-01-01T01:00:00.594+00:00,1.35124,1.35065,1.5,0.75"},
                new string[] { "Symbol3", "2018-01-01T04:00:00.594+00:00,1.35124,1.35065,1.5,0.75"}
            },
            "Symbol2" // Symbol 2 is the oldest
        },

          // Test 4
        new object[] { 
            // Six events, 
            new List<string[]>() {
                new string[] { "Symbol1", "2018-01-01T02:00:00.594+00:00,1.35104,1.35065,1.5,0.75"},
                new string[] { "Symbol2", "2018-01-01T01:00:00.594+00:00,1.35124,1.35065,1.5,0.75"},
                new string[] { "Symbol3", "2018-01-01T04:00:00.594+00:00,1.35124,1.35065,1.5,0.75"},
                new string[] { "Symbol4", "2018-01-01T05:00:00.594+00:00,1.35124,1.35065,1.5,0.75"},
                new string[] { "Symbol5", "2018-01-01T06:00:00.594+00:00,1.35124,1.35065,1.5,0.75"},
                new string[] { "Symbol6", "2018-01-01T07:00:00.594+00:00,1.35124,1.35065,1.5,0.75"}
            },
            "Symbol2" // Symbol 2 is the oldest
        }
    };

    [Theory]
    [MemberData(nameof(Data))]
    public async Task TestBufferOrder(List<string[]> input, string expectedString){

        // Tests that the oldest price items 
        // are taken off the buffer first

        var envMock = TestEnvironment.SetEnvironmentVariables(); 

        // Grab all the symbols in the test and add the into the
        // environment array "symbols"
        List<string> symbols = new List<string>();
        foreach(var item in input){
            symbols.Add(item[0]);
        }

        envMock.SetupGet<string[]>(x=>x.symbols).Returns(symbols.ToArray());

        var ingestObj = new Mock<Ingest>(envMock.Object){
            CallBase = true
        }.Object;

        foreach(var item in input){
            ingestObj.PopulateLocalBuffer(item[0], item[1]);
        }

        BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        await ingestObj.GetOldestItemOffBuffer(buffer);
        IList<PriceObj>? items;
        var output = buffer.TryReceiveAll(out items);

        // There should always be output
        Assert.True(output);

        // Should only have one output value
        Assert.Equal(1, items?.Count);

        // The output event should equal the expected string
        Assert.Equal(expectedString, items?.First().symbol);

    }
    
    [Fact]
    public async void TestingReadFile()
    {
        var envMock = TestEnvironment.SetEnvironmentVariables(); 
        var consumerMock = new Mock<IConsumer>();
        var ingestMock = new Mock<Ingest>(envMock.Object){
            CallBase = true
        };

        ingestMock.Setup(x=>x.EnvironmentSetup());
        consumerMock.Setup<Task>(x=>x.ConsumeAsync(It.IsAny<BufferBlock<PriceObj>>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.FromResult(0));

        var taskManagerMock = new Mock<TaskManager>(consumerMock.Object, ingestMock.Object); // can't mock program

        ingestMock.Object.fileNames.Add(Path.Combine(PathUtil.GetTestPath("TestEnvironmentSetup"), "testSymbol.csv"));

        await taskManagerMock.Object.IngestAndConsume();

        BufferBlock<PriceObj> buffer = taskManagerMock.Object.buffer;
        IList<PriceObj>? items;
        var output = buffer.TryReceiveAll(out items);

        Assert.True(items!=null && items.Count == 1);
    }
}