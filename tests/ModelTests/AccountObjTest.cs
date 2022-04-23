using System.Threading.Tasks;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Utilities;
using Xunit;

namespace Tests;

public class AccountObjTests
{

    public static ServiceProvider Setup(int maximumDrawndownPercentage){
        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        environmentMock.SetupGet<string>(x=>x.accountEquity).Returns("500");
        environmentMock.SetupGet<string>(x=>x.scalingFactor).Returns("TestEnvironmentSetup,1;");
        environmentMock.SetupGet<string>(x=>x.maximumDrawndownPercentage).Returns(maximumDrawndownPercentage.ToString());

        return new ServiceCollection()
            .AddSingleton<IEnvironmentVariables>(environmentMock.Object)
            .AddSingleton<ITradingObjects, TradingObjects>()
            .BuildServiceProvider(true);
    }

    [Fact]
    public void PopulateAccountObjTest(){

        var provider = Setup(maximumDrawndownPercentage: 50);

        var tradingObject = provider.GetService<ITradingObjects>();

        Assert.Equal(500, tradingObject?.accountObj.openingEquity);
        Assert.Equal(500, tradingObject?.accountObj.pnl);
        Assert.Equal(50, tradingObject?.accountObj.maximumDrawndownPercentage);

        tradingObject?.tradeHistory.TryAdd("1", new TradeHistoryObject(){
            profit=10
        });

        Assert.Equal(510, tradingObject?.accountObj.pnl);
        Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

        tradingObject?.tradeHistory.TryAdd("2", new TradeHistoryObject(){
            profit=0
        });

        Assert.Equal(510, tradingObject?.accountObj.pnl);
        Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

        tradingObject?.tradeHistory.TryAdd("3", new TradeHistoryObject(){
            profit=100
        });

        Assert.Equal(610, tradingObject?.accountObj.pnl);
        Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());


        tradingObject?.tradeHistory.TryAdd("4", new TradeHistoryObject(){
            profit=-210
        });

        Assert.Equal(400, tradingObject?.accountObj.pnl);
        Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

        tradingObject?.tradeHistory.TryAdd("5", new TradeHistoryObject(){
            profit=-200
        });

        Assert.Equal(200, tradingObject?.accountObj.pnl);
        Assert.True(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

    }

    [Fact]
    public void TestVariousDrawdownPercentages(){

        var provider = Setup(maximumDrawndownPercentage: 10);

        var tradingObject = provider.GetService<ITradingObjects>();

        Assert.Equal(500, tradingObject?.accountObj.openingEquity);
        Assert.Equal(500, tradingObject?.accountObj.pnl);
        Assert.Equal(10, tradingObject?.accountObj.maximumDrawndownPercentage);

        tradingObject?.tradeHistory.TryAdd("5", new TradeHistoryObject(){
            profit=-60
        });

        Assert.Equal(440, tradingObject?.accountObj.pnl);
        Assert.True(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

    }

    [Fact]
    public void TestCalculateProfitBUY(){

        var provider = Setup(maximumDrawndownPercentage: 10);

        var tradingObject = provider.GetService<ITradingObjects>();
        var envVariables = provider.GetService<IEnvironmentVariables>();

        // Test a BUY
        // Entry at 120 (ASK), current level 100 (BID) = -19 profit (as slippage made the price lower)
        var currentLevel = 80m;
        var priceObj = new PriceObj() {
            symbol="TestEnvironmentSetup",
            ask=100,
        };

        // Create a trade request object to open a trade
        var request = new RequestObject(priceObj, TradeDirection.BUY, envVariables) {
            size = 1
        };

        var output = tradingObject?.accountObj.CalculateProfit(currentLevel, request);

        Assert.Equal(-19,output);


        // Test a BUY
        // Entry at 100 (ASK), current level 120 (BID) = 20 profit
        currentLevel = 120m;
        priceObj = new PriceObj() {
            symbol="TestEnvironmentSetup",
            ask=100,
        };

        // Create a trade request object to open a trade
        request = new RequestObject(priceObj, TradeDirection.BUY, envVariables) {
            size = 1
        };

        output = tradingObject?.accountObj.CalculateProfit(currentLevel, request);

        Assert.Equal(21,output);

    }

    [Fact]
    public void TestCalculateProfitSELL(){

        var provider = Setup(maximumDrawndownPercentage: 10);
        

        var tradingObject = provider.GetService<ITradingObjects>();
        var envVariables = provider.GetService<IEnvironmentVariables>();


        // Test a BUY
        // Entry at 120 (bid), current level 100 (ask) = 20 profit
        var currentLevel = 100m;
        var priceObj = new PriceObj() {
            symbol="TestEnvironmentSetup",
            bid=120,
        };

        // Create a trade request object to open a trade
        var request = new RequestObject(priceObj, TradeDirection.SELL, envVariables) {
            size = 1
        };

        var output = tradingObject?.accountObj.CalculateProfit(currentLevel, request);

        Assert.Equal(21, output);

    }
}