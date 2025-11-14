using System;
using System.Linq;
using backtesting_engine;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Utilities;
using Xunit;


namespace Tests;

[Collection("Sequential")]
public class ExtensionTests
{

    [Fact]
    public void DictionaryKeyStringsTests(){

        // Arrange
        var currentDt = DateTime.Now;

        var priceObj = new PriceObj() {
            symbol="symbolName",
            date=currentDt
        };

        // Act
        var output = DictionaryKeyStrings.OpenTrade(priceObj.symbol,priceObj.date);

        // Assert
        Assert.Contains(priceObj.symbol + "-" + priceObj.date, output);
    }

    [Fact]
    public void ServiceExtensionTestsInvalidStrategy(){

        // Arrange
        TestEnvironment.SetEnvironmentVariables(); 

        // Act
        Func<IServiceCollection> act = () => new ServiceCollection().RegisterStrategies();

        // Assert
        Assert.Throws<ArgumentException>(act);

        // Clean
        TestEnvironment.CleanEnvironment();
    }
   
    [Fact]
    public void ServiceExtensionTestsValidStrategy(){
       
        // Arrange
        TestEnvironment.SetEnvironmentVariables(); 
        EnvironmentVariables.Inject(TradingVariables.STRATEGY, "RandomStrategy");
        var collection = new ServiceCollection().RegisterStrategies();

        // Act
        var response = collection.All(x=> x.ImplementationType!=null && x.ImplementationType.Name == "RandomStrategy");

        // Assert
        Assert.True(response);

        // Clean
        TestEnvironment.CleanEnvironment();
    }

}