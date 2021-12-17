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
        return (T)field?.GetValue(obj);
    }
}

public class IngestTests
{

    [Fact]
    public void CheckEnvironmentVariables()
    {
        // // Arrange
        var mockRepo = new Mock<Input>();

        mockRepo.Protected()
        .Setup<Task>("CoordinateFileRead", ItExpr.IsAny<Dictionary<string, StreamReader>>(), ItExpr.IsAny<BufferBlock<PriceObj>>())
        .Returns(Task.FromResult(true));

        mockRepo.Protected()
        .Setup<StreamReader>("NewMethod", ItExpr.IsAny<string>())
        .Returns(StreamReader.Null);

         mockRepo.Protected()
        .Setup("Cleanup");

        var obj = mockRepo.Object;

        var test = obj.ReadLines(new string[]{"test"}, new BufferBlock<PriceObj>());

        Dictionary<string, StreamReader> streamDic = obj.GetFieldValue<Dictionary<string, StreamReader>>("streamDictionary");
        
        Assert.Equal(streamDic.Count(), 1);

        // //  var result = MethodToTest(mockRepo.Object);
        
        // // Act

        // // Assert
        // Assert.Equal(input, output);
    }

}