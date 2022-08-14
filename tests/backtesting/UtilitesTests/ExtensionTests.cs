using System;
using System.Linq;
using backtesting_engine;
using Microsoft.Extensions.DependencyInjection;
using Utilities;
using Xunit;

namespace Tests;

public class ExtensionTests
{

    [Fact]
    public void DictionaryKeyStringsTests(){

        var currentDt = DateTime.Now;

        var priceObj = new PriceObj() {
            symbol="symbolName",
            date=currentDt
        };

        var output = DictionaryKeyStrings.OpenTrade(priceObj.symbol,priceObj.date);
        Assert.Contains(priceObj.symbol + "-" + priceObj.date, output);
    }

    [Fact]
    public void ServiceExtensionTestsInvalidStrategy(){

        var envMock = TestEnvironment.SetEnvironmentVariables(); 

        // Strategy that doesn't exist
        envMock.SetupGet<string>(x=>x.strategy).Returns("doesntExist");
        Func<IServiceCollection> act = () => new ServiceCollection().RegisterStrategies(envMock.Object);
        Assert.Throws<ArgumentException>(act);
    }
   
    [Fact]
    public void ServiceExtensionTestsValidStrategy(){
        
        var envMock = TestEnvironment.SetEnvironmentVariables(); 

        // Strategy that does exist, case sensitive
        envMock.SetupGet<string>(x=>x.strategy).Returns("RandomWithCloseAtHhll");
        var collection = new ServiceCollection().RegisterStrategies(envMock.Object);
        var response = collection.All(x=> x.ImplementationType!=null && x.ImplementationType.Name == "RandomWithCloseAtHhll");
        Assert.True(response);
        
    }
}