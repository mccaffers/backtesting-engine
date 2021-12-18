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
using Xunit;

namespace Tests;

public static class ReflectionExtensions {
    public static T GetFieldValue<T>(this object obj, string name) {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        if (field?.GetValue(obj) is T myValue)
            return myValue;
       
        return default!;
    }
}

public class IngestTests
{

    [Theory]
    [InlineData("FileName1")]
    [InlineData("FileName1", "FileName2")]
    [InlineData("FileName1", "FileName2", "FileName3")]
    public void CheckStreamDictionaryContainsAllFiles(params string[] fileNames)
    {
        // Arrange
        var mockRepo = new Mock<Input>();

        // stub out the CoordinatedFileRead so the method doesn't try to read from file
        mockRepo.Protected()
        .Setup<Task>("CoordinateFileRead", ItExpr.IsAny<Dictionary<string, StreamReader>>(), ItExpr.IsAny<BufferBlock<PriceObj>>())
        .Returns(Task.FromResult(true));

        mockRepo.Protected()
        .Setup<StreamReader>("ReadFile", ItExpr.IsAny<string>())
        .Returns(StreamReader.Null);

         mockRepo.Protected()
        .Setup("Cleanup");

        var obj = mockRepo.Object;

        // Act
        var test = obj.ReadLines(fileNames, new BufferBlock<PriceObj>());
        
        // Assert
        Dictionary<string, StreamReader> streamDic = obj.GetFieldValue<Dictionary<string, StreamReader>>("streamDictionary");
        Assert.Equal(fileNames.Count(), streamDic.Count());

    }

}