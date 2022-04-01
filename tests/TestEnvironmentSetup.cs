using System;
using Tests;

class TestEnvironment {

    public static string folderPath {get;set;} = PathUtil.GetTestPath("TestEnvironmentSetup");
    public static string[] fileNames {get;set;} = new string[]{"testSymbol.csv"};

    public static void SetEnvironmentVariables(){
        Environment.SetEnvironmentVariable("symbols", "TestEnvironmentSetup");
        Environment.SetEnvironmentVariable("scalingFactor", "TestEnvironmentSetup,1000;");
        Environment.SetEnvironmentVariable("symbolFolder", "Resources");
        Environment.SetEnvironmentVariable("strategy", "random");
        Environment.SetEnvironmentVariable("runID", "debug");
        Environment.SetEnvironmentVariable("elasticUser", "debug");
        Environment.SetEnvironmentVariable("elasticPassword", "debug");
        Environment.SetEnvironmentVariable("elasticCloudID", "debug");
        Environment.SetEnvironmentVariable("accountEquity", "debug");
        Environment.SetEnvironmentVariable("stopDistanceInPips", "10");
        Environment.SetEnvironmentVariable("limitDistanceInPips", "10");
        Environment.SetEnvironmentVariable("maximumDrawndownPercentage", "debug");
        Environment.SetEnvironmentVariable("s3Bucket", "debug");
        Environment.SetEnvironmentVariable("s3Path", "debug");
        Environment.SetEnvironmentVariable("years", "2000");
        Environment.SetEnvironmentVariable("reportingEnabled", "false");
    }
}