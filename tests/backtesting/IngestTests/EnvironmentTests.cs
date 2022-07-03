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

public class IngestEnvironmentTests
{
    [Fact]
    public void TestFilePath(){

        var envMock = TestEnvironment.SetEnvironmentVariables(); 
        var ingestMock = new Mock<Ingest>(envMock.Object){
            CallBase = true
        };

        // Environment.SetEnvironmentVariable("folderPath", PathUtil.GetTestPath(""));

        ingestMock.Object.EnvironmentSetup();

        Assert.True(ingestMock.Object.fileNames.All(x=>x.Contains(TestEnvironment.folderPath)));
        Assert.True(ingestMock.Object.fileNames.Count == TestEnvironment.fileNames.Length);
    }  

}

