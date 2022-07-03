using System;
using System.Collections;
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
        
    static string symbolName="TestEnvironmentSetup";

    public static ServiceProvider Setup(decimal accountEquity)
    {
        // Arrange Environment Variables
        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 

        // Explicitly set the trading environment
        environmentMock.SetupGet<string>(x=>x.accountEquity).Returns(accountEquity.ToString());
        environmentMock.SetupGet<string>(x=>x.scalingFactor).Returns("TestEnvironmentSetup,1;");

        var environmentObj = environmentMock.Object;

        // Setup local dependency provider
        return new ServiceCollection()
            .AddSingleton<IEnvironmentVariables>(environmentObj)
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IElasticClient>(new Mock<IElasticClient>().Object)
            .AddSingleton<IReporting, Reporting>()
            .AddSingleton<ICloseOrder, CloseOrder>()
            .AddSingleton<IOpenOrder, OpenOrder>()
            .AddSingleton<IPositions, Positions>()
            .BuildServiceProvider(true);
    }

    public static IEnumerable<object[]> Data =>
    new List<object[]>
    {
        // fields: ask, bid, account equity, direction
        new object[] {120m, 150m, 100m, "BUY"}, // positive test
        new object[] {100m, 50m, 200m, "BUY"}, // different account opening
        new object[] {100m, 20m, 1000m, "BUY"}, // large initial account
        new object[] {100m, 200m, 100m, "BUY"}, // negative test
        new object[] {100m, 200m, 100m, "SELL"}, // positive test
        new object[] {200m, 100m, 100m, "SELL"}, // negative test
    };

    [Theory]
    [MemberData(nameof(Data))]
    public void BuyCalculationsTest(decimal ask, decimal bid, decimal accountEquity, string direction) {

        TradeDirection tradeDirection = (TradeDirection)Enum.Parse(typeof(TradeDirection), direction);

        var provider = Setup(accountEquity);
        var tradingObject = provider.GetService<ITradingObjects>();
        var positions = provider.GetService<IPositions>();
        var envVariables = provider.GetService<IEnvironmentVariables>() ?? new EnvironmentVariables();
        var openOrder = provider.GetService<IOpenOrder>();

        // Act
        // Inital price event
        var priceObj = new PriceObj() {
            symbol=symbolName,
            ask=ask,
            bid=bid,
        };

        var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

        // Create a trade request object to open a trade
        openOrder?.Request(new RequestObject(priceObj,
                                             tradeDirection,
                                             envVariables, key) {
            size = 1,
        });

        // Create a new price event
        var priceObjNext = new PriceObj(){
            symbol=symbolName,
            bid=bid,
            ask=ask
        };

        positions?.Review(priceObjNext);

        var slippage = 1m / envVariables?.GetScalingFactor(symbolName);

        var expectedPnL = accountEquity - ( (ask - slippage) - bid);
        if(tradeDirection == TradeDirection.SELL){
            expectedPnL = ( (bid+slippage) - ask ) + accountEquity;
        }

        // Assert
        Assert.Equal(1, tradingObject?.tradeHistory.Count); // one record has been added
        Assert.Equal(expectedPnL, tradingObject?.accountObj.pnl); // one record has been added
    }

    
}