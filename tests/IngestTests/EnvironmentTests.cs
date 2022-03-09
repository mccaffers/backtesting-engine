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
    [Fact(Skip="TODO")]
    public void TestFilePath(){

        TestEnvironment.SetEnvironmentVariables(); 

        var programMock = new Mock<Main>(); // can't mock program
        var consumerMock = new Mock<IConsumer>();
        var inputMock = new Mock<Ingest>(){
            CallBase = true
        };

        Assert.Equal(TestEnvironment.folderPath, inputMock.Object.folderPath);
    }  

}

[Collection("Sequential")]
public class IngestEnvironmentTests2
{

    [Fact(Skip="TODO")]
    public void TestEnvironmentSetup(){

        TestEnvironment.SetEnvironmentVariables(); 

        var inputMock = new Mock<Ingest>(){
            CallBase = true
        };
        
        inputMock.Object.EnvironmentSetup();

        var myFiles = new List<string>();
        foreach(var file in TestEnvironment.fileNames){
            myFiles.Add(Path.Combine(PathUtil.GetTestPath("TestEnvironmentSetup"), file));
        }

        Assert.True(myFiles.SequenceEqual(inputMock.Object.fileNames));
    }

}

