using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backtesting_engine;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using backtesting_engine_operations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using Utilities;
using Xunit;

namespace Tests;

public class PositionTests
{
    
    [Fact]
    public async Task PositionsReviewTest() {

        // Arrange Environment Variables
        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        environmentMock.SetupGet<bool>(x=>x.loadFromEnvironmnet).Returns(false);
        environmentMock.SetupGet<string>(x=>x.scalingFactor).Returns("TestEnvironmentSetup,1;");

        var environmentObj = environmentMock.Object;

        // Setup local dependency provider
        var provider = new ServiceCollection()
            .AddSingleton<IEnvironmentVariables>(environmentObj)
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IElasticClient>(new Mock<IElasticClient>().Object)
            .AddSingleton<IReporting, Reporting>()
            .AddSingleton<ICloseOrder, CloseOrder>()
            .AddSingleton<IPositions, Positions>()
            .BuildServiceProvider(true);

        // Pull the trading objects from the service provider
        var tradingObject = provider.GetService<ITradingObjects>();
        var positions = provider.GetService<IPositions>();

        string symbolName="TestEnvironmentSetup";

        // Act
        // Generate from price events

        // Inital price event
        var priceObj = new PriceObj(){
            date=DateTime.Now,
            symbol=symbolName,
            bid=100,
            ask=120
        };

        // Create a trade request object to open a trade
        var reqObj = new RequestObject(priceObj)
        {
            direction = TradeDirection.BUY,
            size = 1,
            stopDistancePips = 20,
            limitDistancePips = 20,
        };

        // Add it to the concurrent dictionary
        tradingObject?.openTrades.TryAdd(reqObj.key, reqObj);

        // Create a new price event
        var priceObjNext = new PriceObj(){
            date=DateTime.Now,
            symbol=symbolName,
            bid=150,
            ask=130
        };

        positions?.Review(priceObjNext);
        
        // Assert
        Assert.Equal(1, tradingObject?.tradeHistory.Count); // one record has been added
        Assert.Equal(30, tradingObject?.accountObj.pnl); // one record has been added


    }

}