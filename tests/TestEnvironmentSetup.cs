using System;
using Tests;

class TestEnvironment {

    public static string folderPath {get;set;} = PathUtil.GetTestPath("TestEnvironmentSetup");
    public static string[] fileNames {get;set;} = new string[]{"testSymbol.csv"};

    public static void SetEnvironmentVariables(){
        Environment.SetEnvironmentVariable("symbols", "TestEnvironmentSetup");
        Environment.SetEnvironmentVariable("TestEnvironmentSetup" + "_SF", "1000");
        Environment.SetEnvironmentVariable("folderPath", PathUtil.GetTestPath(""));
        Environment.SetEnvironmentVariable("strategy", "random");
        Environment.SetEnvironmentVariable("runID", "debug");
        Environment.SetEnvironmentVariable("elasticUser", "debug");
        Environment.SetEnvironmentVariable("elasticPassword", "debug");
        Environment.SetEnvironmentVariable("elasticCloudID", "debug");
        Environment.SetEnvironmentVariable("accountEquity", "debug");
        Environment.SetEnvironmentVariable("maximumDrawndownPercentage", "debug");
    }
}