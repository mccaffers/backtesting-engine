
// using backtesting_engine;
// using backtesting_engine.interfaces;
// using backtesting_engine_models;
// using backtesting_engine_operations;
// using Moq;
// using Utilities;
// using Xunit;

// namespace Tests;

// [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
// public class RequestOpenTradesTests
// {
        
//     static string symbolName="TestEnvironmentSetup";

//     [Fact]
//     public void TestSlippage(){

//         // Arrange
//         TestEnvironment.SetEnvironmentVariables(); 

//         EnvironmentVariables.VariableInjectList.TryAdd(TradingVariables.SCALING_FACRTOR, "TestEnvironmentSetup,1;");

//         var openOrderMock = new Mock<IOpenOrder>();

//         var priceObj = new PriceObj() {
//             symbol=symbolName,
//             ask=100,
//             bid=120,
//         };

//         RequestObject? receivedReqObj = null;
//         openOrderMock.Setup(x=>x.Request(It.IsAny<RequestObject>())).Callback( (RequestObject callbackReq) => {
//             receivedReqObj=callbackReq;
//         }); // Stubbed out call, so the request doesn't go anywhere

//         var requestOpenTradeMock = new Mock<RequestOpenTrade>(openOrderMock.Object);
        
//         // Create a trade request object to open a trade
//         var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);
//         var scalingFactor = EnvironmentVariables.GetScalingFactor(priceObj.symbol);
//         var reqObj = new RequestObject(priceObj, TradeDirection.BUY, key, scalingFactor) {
//             size = 1,
//         };

//         requestOpenTradeMock.Object.Request(reqObj);

//         Assert.Equal(reqObj.priceObj, receivedReqObj?.priceObj);
//         Assert.Equal(priceObj.ask, receivedReqObj?.level); // slippage test


//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

//     [Fact]
//     public void TestDirection(){

//         // Arrange
//         TestEnvironment.SetEnvironmentVariables(); 

//         EnvironmentVariables.VariableInjectList.TryAdd(TradingVariables.SCALING_FACRTOR, "TestEnvironmentSetup,1;");

//         var openOrderMock = new Mock<IOpenOrder>();

//         var priceObj = new PriceObj() {
//             symbol=symbolName,
//             ask=100,
//             bid=120,
//         };

//         RequestObject? receivedReqObj = null;
//         openOrderMock.Setup(x=>x.Request(It.IsAny<RequestObject>())).Callback( (RequestObject callbackReq) => {
//             receivedReqObj=callbackReq;
//         }); // Stubbed out call, so the request doesn't go anywhere

//         var requestOpenTradeMock = new Mock<RequestOpenTrade>(openOrderMock.Object );

//         var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

//         // Create a trade request object to open a trade
//         var scalingFactor = EnvironmentVariables.GetScalingFactor(priceObj.symbol);
//         var reqObj = new RequestObject(priceObj, TradeDirection.BUY, key, scalingFactor) {
//             size = 1,
//         };

//         requestOpenTradeMock.Object.Request(reqObj);
//         Assert.Equal(priceObj.ask, receivedReqObj?.level);

//         // Create a trade request object to open a trade
//         reqObj = new RequestObject(priceObj, TradeDirection.SELL, key, scalingFactor) {
//             size = 1,
//         };

//         requestOpenTradeMock.Object.Request(reqObj);
//         Assert.Equal(priceObj.bid, receivedReqObj?.level);

//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

//     [Fact]
//     public void TestDistance(){

//         // The Open Request Method should convert distance in pips to the correct
//         // STOP/LIMIT values alongside slippage base on the direction of the trde

//         // Arrange
//         TestEnvironment.SetEnvironmentVariables(); 

//         EnvironmentVariables.VariableInjectList.TryAdd(TradingVariables.SCALING_FACRTOR, "TestEnvironmentSetup,1;");

//         var openOrderMock = new Mock<IOpenOrder>();

//         var priceObj = new PriceObj() {
//             symbol=symbolName,
//             ask=100,
//             bid=120,
//         };

//         RequestObject? receivedReqObj = null;
//         openOrderMock.Setup(x=>x.Request(It.IsAny<RequestObject>())).Callback( (RequestObject callbackReq) => {
//             receivedReqObj=callbackReq;
//         }); // Stubbed out call, so the request doesn't go anywhere

//         var requestOpenTradeMock = new Mock<RequestOpenTrade>(openOrderMock.Object);
//         var scalingFactor = EnvironmentVariables.GetScalingFactor(priceObj.symbol);
//         var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

//         // Create a trade request object to open a trade
//         var reqObj = new RequestObject(priceObj, TradeDirection.BUY, key, scalingFactor) {
//             size = 1,
//             stopDistancePips = 10,
//             limitDistancePips = 10
//         };

//         requestOpenTradeMock.Object.Request(reqObj);

//         Assert.Equal(10, receivedReqObj?.StopDistancePips);
//         Assert.Equal(10, receivedReqObj?.limitDistancePips);

//         // For a BUY order
//         // ASK is subtracted by the slippage
//         // ASK is subtracted by the stop distance in PIPs multiplied the scaling factor
//         // This equals the STOP limit for the BUY ORDER
//         Assert.Equal(priceObj.ask-(reqObj.StopDistancePips*1), receivedReqObj?.stopLevel); 

//         // ASK is subtracted by the slippage
//         // ASK is increased by the limit distance in PIPs multiplied the scaling factor
//         // This equals the Limit for the BUY ORDER
//         Assert.Equal(priceObj.ask+(reqObj.limitDistancePips*1), receivedReqObj?.limitLevel); 

//         priceObj = new PriceObj() {
//             symbol=symbolName,
//             ask=100,
//             bid=120,
//         };

//         // Create a trade request object to open a trade
//         reqObj = new RequestObject(priceObj, TradeDirection.SELL, key, scalingFactor) {
//             size = 1,
//             stopDistancePips = 10,
//             limitDistancePips = 10
//         };

//         requestOpenTradeMock.Object.Request(reqObj);

//         Assert.Equal(10, receivedReqObj?.StopDistancePips);
//         Assert.Equal(10, receivedReqObj?.limitDistancePips);

//         // For a SELL order
//         // BID is subtracted by the slippage
//         // BID is increased by the stop distance in PIPs, times the scaling factor
//         // This equals the STOP limit for the BUY ORDER
//         Assert.Equal(priceObj.bid+(reqObj.StopDistancePips*1), receivedReqObj?.stopLevel); 

//         // BID is subtracted by the slippage
//         // BID is subtracted by the limit distance in PIPs multiplied the scaling factor
//         // This equals the Limit for the BUY ORDER
//         Assert.Equal(priceObj.bid-(reqObj.limitDistancePips*1), receivedReqObj?.limitLevel); 


//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

// }
