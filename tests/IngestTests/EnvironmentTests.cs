using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using backtesting_engine;
using backtesting_engine_ingest;
using Moq;
using Utilities;
using Xunit;

namespace Tests;

[Collection("Sequential")]
public class IngestEnvironmentTests
{
    [Fact]
    public void TestFilePath(){

        var folderPath = "testFilePath";

        var mySymbols = new List<string>();
        mySymbols.Add("testSymbol");

        var key = "symbols";
        var input = "TestEnvironmentSetup";
        Environment.SetEnvironmentVariable(key, input);

        key = input + "_SF";
        input = "1000";
        Environment.SetEnvironmentVariable(key, input);

        key = "folderPath";
        input = PathUtil.GetTestPath("");
        Environment.SetEnvironmentVariable(key, input);

        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(){
            CallBase = true
        };

        Assert.Equal(folderPath, inputMock.Object.folderPath);
    }  

}

[Collection("Sequential")]
public class IngestEnvironmentTests2
{

    [Fact]
    public void TestEnvironmentSetup(){

        // var mySymbols = new List<string>();
        // mySymbols.Add("TestEnvironmentSetup");

        var key = "symbols";
        var input = "TestEnvironmentSetup";
        Environment.SetEnvironmentVariable(key, input);

        key = "folderPath";
        input = PathUtil.GetTestPath("");
        Environment.SetEnvironmentVariable(key, input);

        var inputMock = new Mock<Ingest>(null, null){
            CallBase = true
        };
        
        inputMock.Object.EnvironmentSetup();

        var myFiles = new List<string>();
        myFiles.Add(Path.Combine(PathUtil.GetTestPath("TestEnvironmentSetup"), "testSymbol.csv"));
        Assert.True(myFiles.SequenceEqual(inputMock.Object.fileNames));
    }

}

[Collection("Sequential")]
public class IngestEnvironmentTests3
{   
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
}

