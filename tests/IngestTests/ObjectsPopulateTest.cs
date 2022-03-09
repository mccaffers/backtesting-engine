using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_ingest;
using Moq;
using Moq.Protected;
using Utilities;
using Xunit;

namespace Tests;
public class ObjectsPopulateTest
{


    [Theory]
    [InlineData("FileName1")]
    [InlineData("FileName1", "FileName2")]
    [InlineData("FileName1", "FileName2", "FileName3")]
    public async void CheckStreamDictionaryContainsAllFiles(params string[] fileNames)
    {
        // Arrange
        TestEnvironment.SetEnvironmentVariables(); 

        var inputMock = new Mock<Ingest>();

        // stub out the CoordinatedFileRead so the method doesn't try to read from file
        inputMock.Protected()
        .Setup<Task>("LoopStreamDictionaryAndReadLine", ItExpr.IsAny<Dictionary<string, StreamReader>>(), ItExpr.IsAny<BufferBlock<PriceObj>>())
        .Returns(Task.FromResult(true));

        // stub out the StreamReader method so the method doesn't try to read from file
        inputMock.Protected()
        .Setup<StreamReader>("ReadFile", ItExpr.IsAny<string>())
        .Returns(StreamReader.Null);

        // stub out the StreamReader Cleanup method so the method doesn't try to dipose of the streamreader
        inputMock.Protected()
        .Setup("StreamReaderCleanup");

        // inputMock.Object.EnvironmentSetup();

        // inputMock.Protected().SetupGet<IEnumerable<string>>("fileNames").Returns(fileNames);
        inputMock.SetupGet(x => x.fileNames).Returns(fileNames.ToList());
        // inputMock.Object.fileNames = fileNames.ToList();

        // Act
        var inputObj = inputMock.Object;
        await inputObj.ReadLines(new BufferBlock<PriceObj>(), It.IsAny<CancellationToken>());
        
        // Assert
        Dictionary<string, StreamReader> streamDic = inputObj.streamDictionary;
        Assert.Equal(fileNames.Length, streamDic.Count);

    }

}