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

        TestEnvironment.SetEnvironmentVariables(); 

        var inputMock = new Mock<Ingest>(){
            CallBase = true
        };

        Environment.SetEnvironmentVariable("folderPath", PathUtil.GetTestPath(""));

        inputMock.Object.EnvironmentSetup();

        Assert.True(inputMock.Object.fileNames.All(x=>x.Contains(TestEnvironment.folderPath)));
        Assert.True(inputMock.Object.fileNames.Count == TestEnvironment.fileNames.Count());
    }  

}

