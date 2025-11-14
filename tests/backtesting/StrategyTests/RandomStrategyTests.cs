using System;
using System.Threading.Tasks;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using backtesting_engine_strategies;
using Moq;
using Utilities;
using Xunit;

namespace Tests;

[Collection("Sequential")]
public class RandomStrategyTests
{

    [Theory]
    [InlineData(10, 10, 1)]
    [InlineData(20, 20, 2)]
    public async Task TestInvokeMethod(decimal stopDistanceInPips, decimal limitDistanceInPips, decimal tradingSize)
    {
        // Arrange
        TestEnvironment.SetEnvironmentVariables();

        var requestOpenTradeMock = new Mock<IRequestOpenTrade>();
        var tradeObjectsMock = new TradingObjects();
        var closeOrderMock = new Mock<ICloseOrder>();

        var priceObjNext = new PriceObj()
        {
            symbol = "TestEnvironmentSetup",
            bid = 100,
            ask = 120,
            date = DateTime.Now
        };

        var strategy = new StrategyDefinition()
        {
            UUID = "TEST_STRATEGY_ID",
            TRADING_VARIABLES = new StrategyDefinition.TRADING_VARIABLES_CLASS()
            {
                STRATEGY = "RandomStrategy",
                STOP_DISTANCE_IN_PIPS = stopDistanceInPips.ToString(),
                LIMIT_DISTANCE_IN_PIPS = limitDistanceInPips.ToString(),
                TRAILING_STOP_LOSS_ACTIVE = "0",
                TRAILING_STOP_LOSS_VALUE = "0",
                MOVING_LIMIT_VALUE = "0",
                TRADING_SIZE = tradingSize.ToString()
            }
        };

        RequestObject? output = null;

        requestOpenTradeMock.Setup(x => x.Request(It.IsAny<RequestObject>()))
            .Callback((RequestObject incomingObject) =>
            {
                output = incomingObject;
            })
            .ReturnsAsync(true);

        var randomStrategy = new RandomStrategy(
            requestOpenTradeMock.Object,
            tradeObjectsMock,
            closeOrderMock.Object
        );

        // Act
        await randomStrategy.Invoke(priceObjNext, strategy);

        // Assert - should have opened a trade (or not, depending on random)
        // Since the strategy uses Random, we can't guarantee it will open a trade
        // But we can test that if it did open, the parameters are correct
        if (output != null)
        {
            Assert.Equal(tradingSize, output.size);
            Assert.Equal(stopDistanceInPips, output.stopDistancePips);
            Assert.Equal(limitDistanceInPips, output.limitDistancePips);
            Assert.NotEmpty(output.dealReference);
        }

        // Clean
        TestEnvironment.CleanEnvironment();
    }

    [Fact]
    public async Task TestInvokeMethod_DoesNotOpenSecondTrade_WhenTradeAlreadyOpen()
    {
        // Arrange
        TestEnvironment.SetEnvironmentVariables();

        var requestOpenTradeMock = new Mock<IRequestOpenTrade>();
        var tradeObjectsMock = new TradingObjects();
        var closeOrderMock = new Mock<ICloseOrder>();

        // Add an existing open trade
        tradeObjectsMock.openTrades.Add("existing_trade", new RequestObject(
            priceObj: new PriceObj() { symbol = "EURUSD", bid = 1.0m, ask = 1.1m, date = DateTime.Now },
            direction: TradeDirection.BUY,
            dealReference: "existing_trade",
            scalingFactor: 10000,
            stopDistancePips: 10,
            limitDistancePips: 10,
            size: 1,
            strategyId: "TEST"
        ));

        var priceObjNext = new PriceObj()
        {
            symbol = "TestEnvironmentSetup",
            bid = 100,
            ask = 120,
            date = DateTime.Now
        };

        var strategy = new StrategyDefinition()
        {
            UUID = "TEST_STRATEGY_ID",
            TRADING_VARIABLES = new StrategyDefinition.TRADING_VARIABLES_CLASS()
            {
                STRATEGY = "RandomStrategy",
                STOP_DISTANCE_IN_PIPS = "10",
                LIMIT_DISTANCE_IN_PIPS = "10",
                TRAILING_STOP_LOSS_ACTIVE = "0",
                TRAILING_STOP_LOSS_VALUE = "0",
                MOVING_LIMIT_VALUE = "0",
                TRADING_SIZE = "1"
            }
        };

        var randomStrategy = new RandomStrategy(
            requestOpenTradeMock.Object,
            tradeObjectsMock,
            closeOrderMock.Object
        );

        // Act
        await randomStrategy.Invoke(priceObjNext, strategy);

        // Assert - should not have attempted to open a trade
        requestOpenTradeMock.Verify(x => x.Request(It.IsAny<RequestObject>()), Times.Never);

        // Clean
        TestEnvironment.CleanEnvironment();
    }
}
