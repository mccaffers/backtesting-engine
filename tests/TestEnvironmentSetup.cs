using System;
using Tests;

class TestEnvironment {

    public static string folderPath {get;set;} = PathUtil.GetTestPath("TestEnvironmentSetup");
    public static string[] fileNames {get;set;} = new string[]{"testSymbol.csv"};

    public static void SetEnvironmentVariables(){
        Environment.SetEnvironmentVariable("symbols", "TestEnvironmentSetup");
        Environment.SetEnvironmentVariable("scalingFactor", "TestEnvironmentSetup,1000;");
        Environment.SetEnvironmentVariable("symbolFolder", "Resources");
        Environment.SetEnvironmentVariable("strategy", "debug");
        Environment.SetEnvironmentVariable("runID", "debug");
        Environment.SetEnvironmentVariable("elasticUser", "debug");
        Environment.SetEnvironmentVariable("elasticPassword", "debug");
        Environment.SetEnvironmentVariable("elasticCloudID", "my-deployment:bG9jYWxob3N0JDE3OTIwMDJkLTU0YjQ0NTZhYTNhNTg0M2QwZjgxNWM3OSQ1NGI0NDU2YWEzYTU4NDNkMGY4MTVjNzk=");
        Environment.SetEnvironmentVariable("accountEquity", "100");
        Environment.SetEnvironmentVariable("stopDistanceInPips", "10");
        Environment.SetEnvironmentVariable("limitDistanceInPips", "10");
        Environment.SetEnvironmentVariable("maximumDrawndownPercentage", "50");
        Environment.SetEnvironmentVariable("s3Bucket", "debug");
        Environment.SetEnvironmentVariable("s3Path", "debug");
        Environment.SetEnvironmentVariable("years", "2000");
        Environment.SetEnvironmentVariable("reportingEnabled", "false");
        Environment.SetEnvironmentVariable("runIteration", "0");
    }
}