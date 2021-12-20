using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

         var envMock = new Mock<EnvironmentVariables>();
        envMock.Setup(x=>x.Get("symbols")).Returns("");
        envMock.Setup(x=>x.Get("folderPath")).Returns("");

        var inputMock = new Mock<Ingest>(envMock.Object, fileNames.ToList());

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

        // inputMock.Protected().SetupGet<IEnumerable<string>>("fileNames").Returns(fileNames);

        // Act
        var inputObj = inputMock.Object;
        await inputObj.ReadLines(new BufferBlock<PriceObj>());
        
        // Assert
        Dictionary<string, StreamReader> streamDic = inputObj.streamDictionary;
        Assert.Equal(fileNames.Length, streamDic.Count);

    }

}