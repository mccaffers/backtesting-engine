using System;
using Moq;
using Tests;
using Utilities;

class TestEnvironment {

    public static string folderPath {get;set;} = PathUtil.GetTestPath("TestEnvironmentSetup");
    public static string[] fileNames {get;set;} = new string[]{"testSymbol.csv"};

    public static Mock<IEnvironmentVariables> SetEnvironmentVariables(){

        var environmentMock = new Mock<IEnvironmentVariables>();
        
        environmentMock.SetupGet<string[]>(x=>x.symbols).Returns(new string[]{"TestEnvironmentSetup"});
        environmentMock.SetupGet<string>(x=>x.scalingFactor).Returns("TestEnvironmentSetup,1000;");
        environmentMock.SetupGet<string>(x=>x.symbolFolder).Returns("Resources");
        environmentMock.SetupGet<string>(x=>x.strategy).Returns("debug");
        environmentMock.SetupGet<string>(x=>x.runID).Returns("debug");
        environmentMock.SetupGet<string>(x=>x.elasticUser).Returns("debug");
        environmentMock.SetupGet<string>(x=>x.elasticPassword).Returns("debug");
        environmentMock.SetupGet<string>(x=>x.elasticCloudID).Returns("my-deployment:bG9jYWxob3N0JDE3OTIwMDJkLTU0YjQ0NTZhYTNhNTg0M2QwZjgxNWM3OSQ1NGI0NDU2YWEzYTU4NDNkMGY4MTVjNzk=");
        environmentMock.SetupGet<string>(x=>x.accountEquity).Returns("100");
        environmentMock.SetupGet<string>(x=>x.stopDistanceInPips).Returns("100");
        environmentMock.SetupGet<string>(x=>x.limitDistanceInPips).Returns("10");
        environmentMock.SetupGet<string>(x=>x.maximumDrawndownPercentage).Returns("50");
        environmentMock.SetupGet<string>(x=>x.s3Bucket).Returns("debug");
        environmentMock.SetupGet<string>(x=>x.s3Path).Returns("debug");
        environmentMock.SetupGet<int[]>(x=>x.years).Returns(new int[]{2006});
        environmentMock.SetupGet<bool>(x=>x.reportingEnabled).Returns(false);
        environmentMock.SetupGet<string>(x=>x.runIteration).Returns("0");

        return environmentMock;
    }
}