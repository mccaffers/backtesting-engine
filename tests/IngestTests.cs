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
    [InlineData(true, new string[] { "2018-01-01T22:52:26.862+00:00", "1.20146", "1.20138","1.2","1.3"})]
    [InlineData(false, new string[] {})]
    [InlineData(false, new string[] { "" })]
    [InlineData(false, new string[] { "","","","","" })]
    [InlineData(false, new string[] { "a","a","a","a","a" })]
    [InlineData(false, new string[] { "a","1.2","a","a","a" })]
    public void CheckValidation(bool expectedResult, string[] strings)
    {
        // Act
        var outputResult = Ingest.ArrayHasRightValues(strings);
        Assert.Equal(expectedResult, outputResult);
    }
        
    [Theory]
    [InlineData(true, "2018-01-01T22:52:26.862+00:00")]
    [InlineData(true, "2018-01-01T22:52:26.862")]
    [InlineData(false, "2018-01-01 22:52:26.862")]
    [InlineData(false, "2018-01-01")]
    [InlineData(false, "a")]
    public void CheckDateExtract(bool expectedResult, string dateTimeString)
    {
        // Act
        var outputResult = Ingest.extractDt(dateTimeString).parsed;
        Assert.Equal(expectedResult, outputResult);
    }

    [Fact]
    public void TestFilePath(){

        var folderPath = "testFilePath";

        var mySymbols = new List<string>();
        mySymbols.Add("testSymbol");

         // Arrange
        var envMock = new Mock<EnvironmentVariables>();
        envMock.Setup(x=>x.Get("folderPath")).Returns("testFilePath");
        envMock.Setup(x=>x.Get("symbols")).Returns(String.Join(", ", mySymbols.ToArray()));

        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(envMock.Object, null){
            CallBase = true
        };

        Assert.Equal(folderPath, inputMock.Object.folderPath);
    }

    [Fact]
    public void TestIngestConstructor(){

        var folderPath = "testFilePath";

        var mySymbols = new List<string>();
        mySymbols.Add("testSymbol");

        var key = "symbols";
        var input = "Test";
        Environment.SetEnvironmentVariable(key, input);

        key = "folderPath";
        input = folderPath;
        Environment.SetEnvironmentVariable(key, input);

        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(null, null){
            CallBase = true
        };

        Assert.Equal(folderPath, inputMock.Object.folderPath);
    }

    [Fact]
    public void TestEnvironmentSetup(){

        // var mySymbols = new List<string>();
        // mySymbols.Add("TestEnvironmentSetup");

        var key = "symbols";
        var input = "TestEnvironmentSetup";
        Environment.SetEnvironmentVariable(key, input);

        key = "folderPath";
        input = GetTestPath("");
        Environment.SetEnvironmentVariable(key, input);

        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(null, null){
            CallBase = true
        };
        
        inputMock.Object.EnvironmentSetup();

        var myFiles = new List<string>();
        myFiles.Add(Path.Combine(GetTestPath("TestEnvironmentSetup"), "testSymbol.csv"));

        Assert.True(myFiles.SequenceEqual(inputMock.Object.fileNames));
    }


    public static string GetTestPath(string relativePath)
    {
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath) ?? "";

        if(relativePath.Length > 0){
            return Path.Combine(dirPath, "resources", relativePath);
        } else {
            return Path.Combine(dirPath, "resources");
        }
    }
    
    [Fact]
    public async void TestFile()
    {

        var mySymbols = new List<string>();
        mySymbols.Add("testSymbol");

        var myFiles = new List<string>();
        myFiles.Add(GetTestPath("testSymbol.csv"));

         // Arrange
        var envMock = new Mock<EnvironmentVariables>();
        envMock.Setup(x=>x.Get("symbols")).Returns(String.Join(", ", mySymbols.ToArray()));

        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(envMock.Object, myFiles){
            CallBase = true
        };

        inputMock.Setup(x=>x.EnvironmentSetup());

        consumerMock.Setup<Task>(x=>x.ConsumeAsync(It.IsAny<BufferBlock<PriceObj>>())).Returns(Task.FromResult(0));
        // programMock.Setup(x=>x.EnvironmentSetup());
        // inputMock.Setup(x=>x.ReadLines(It.IsAny<BufferBlock<PriceObj>>())).Returns(Task.CompletedTask);

        var programObj = programMock.Object;
        // programObj.fileNames.Add(GetTestPath("testSymbol.csv"));

        await programObj.IngestAndConsume(consumerMock.Object, inputMock.Object);

        BufferBlock<PriceObj> buffer = programObj.GetFieldValue<BufferBlock<PriceObj>>("buffer");
        IList<PriceObj>? items;
        var output = buffer.TryReceiveAll(out items);

        Assert.True(items!=null && items.Count == 1);
    }

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