using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using backtesting_engine_operations;
using backtesting_engine_strategies;
using Moq;
using Xunit;

namespace Tests;

public class RandomStrategyTests
{

    [Theory(Skip = "Skip for now")]
    [InlineData(10,10,1)]
    [InlineData(20,20,2)]
    public async void TestInvokeMethod(decimal stopDistanceInPips, decimal limitDistanceInPips, decimal tradingSize){

        var envMock = TestEnvironment.SetEnvironmentVariables(); 
        envMock.SetupGet<string>(x=>x.stopDistanceInPips).Returns(stopDistanceInPips.ToString());
        envMock.SetupGet<string>(x=>x.limitDistanceInPips).Returns(limitDistanceInPips.ToString());
        envMock.SetupGet<string>(x=>x.tradingSize).Returns(tradingSize.ToString());

        var requestOpenTradeMock = new Mock<IRequestOpenTrade>();
        var webNotificationMock = new Mock<IWebNotification>();

         var priceObjNext = new PriceObj(){
            symbol="TestEnvironmentSetup",
            bid=100,
            ask=120
        };
        
        RequestObject? output = null;

        requestOpenTradeMock.Setup(x=>x.Request(It.IsAny<RequestObject>())).Callback( ( RequestObject incomingObject) => {
            output=incomingObject;
        });

        var randomStrategyMock = new Mock<RandomStrategy>(requestOpenTradeMock.Object, envMock.Object, webNotificationMock.Object){
            CallBase = true
        };

        await randomStrategyMock.Object.Invoke(priceObjNext);
        priceObjNext.date = priceObjNext.date.AddSeconds(61);
        await randomStrategyMock.Object.Invoke(priceObjNext);

        Assert.Equal(priceObjNext.bid, output?.priceObj.bid);
        Assert.Equal(tradingSize, output?.size);
        Assert.Equal(stopDistanceInPips, output?.stopDistancePips);
        Assert.Equal(limitDistanceInPips, output?.limitDistancePips);
        Assert.NotEmpty(output?.key);
    }
}
