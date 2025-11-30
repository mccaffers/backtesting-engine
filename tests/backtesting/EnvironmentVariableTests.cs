using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Utilities;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = true)]
namespace Tests;

[Collection("Sequential")]
public class EnvironmentVariableTests
{
    [Fact]
    public void CheckEnvironmentVariables()
    {
        System.Console.WriteLine("One started");

        // Arrange
        Environment.SetEnvironmentVariable(TradingVariables.STRATEGY.ToString(), "TEST");
        // EnvironmentVariables.ReloadEnvironmentVariables();

        // Assert
        Assert.Equal("TEST", TradingVariables.STRATEGY.Value());

        // Clean
        TestEnvironment.CleanEnvironment();
    }

    [Fact]
    public void CheckOverride()
    {
        System.Console.WriteLine("Two started");
        // Arrange
        EnvironmentVariables.VariableInjectList.TryAdd(TradingVariables.STRATEGY, "FROM INJECT");
        Environment.SetEnvironmentVariable(TradingVariables.STRATEGY.ToString(), "FROM ENVIRONMENT");

        // Assert
        Assert.Equal("FROM INJECT", TradingVariables.STRATEGY.Value());

        // Clean
        TestEnvironment.CleanEnvironment();
    }

    [Fact]
    public void UseLocalTestFile()
    {
        EnvironmentJson? source; 
        using (StreamReader r = new StreamReader(Path.Combine(PathUtil.GetTestPath("environmentVariables.json"))))
        {  
            string json = r.ReadToEnd();  
            source = JsonSerializer.Deserialize<EnvironmentJson>(json);  
        }  

        if(source != null){
            EnvironmentVariables.Inject(TradingVariables.STRATEGY, source.STRATEGY);
        }

        // Assert
        Assert.Equal("DEBUG", TradingVariables.STRATEGY.Value());
       
       // Clean
        TestEnvironment.CleanEnvironment();
    }

    [Fact]
    public void TestDictionary()
    {

        EnvironmentJson? source; 
        using (StreamReader r = new StreamReader(Path.Combine(PathUtil.GetTestPath("environmentVariables.json"))))
        {  
            string json = r.ReadToEnd();  
            source = JsonSerializer.Deserialize<EnvironmentJson>(json);  
        }  

        if(source != null){
            EnvironmentVariables.Inject(TradingVariables.STRATEGY, source.STRATEGY);
        }
        Environment.SetEnvironmentVariable("TEST", "1");
        Environment.SetEnvironmentVariable("TEST2", "FALSE");
        Environment.SetEnvironmentVariable("TEST3", "no");

        var entries = ReportEnvironmentVariables.Get();

        var stringOutput = entries["STRATEGY"];
        Assert.Equal("DEBUG", stringOutput);
        Assert.True(stringOutput is string);

        var intOutput = entries["TEST"];
        Assert.Equal(1.0m, intOutput);
        Assert.True(intOutput is decimal);
        
        var boolOutput = entries["TEST2"];
        Assert.Equal(false, boolOutput);
        Assert.True(boolOutput is bool);

        var stringOutput2 = entries["TEST3"];
        Assert.Equal("no", stringOutput2);
        Assert.True(stringOutput2 is string);
    }


}

