// using System.Threading.Tasks;
// using backtesting_engine;
// using backtesting_engine.interfaces;
// using backtesting_engine_models;
// using Microsoft.Extensions.DependencyInjection;
// using Moq;
// using Utilities;
// using Xunit;

// namespace Tests;

// [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
// public class AccountObjTests
// {

//     public static ServiceProvider Setup(int maximumDrawndownPercentage){

//         // Arrange
//         TestEnvironment.SetEnvironmentVariables(); 

//         EnvironmentVariables.VariableInjectList.TryAdd(BACKTESTING.ACCOUNT_EQUITY, "500");
//         EnvironmentVariables.VariableInjectList.TryAdd(BACKTESTING.REPORT_INDIVIDUAL_TRADES, "TRUE");
//         EnvironmentVariables.VariableInjectList.TryAdd(BACKTESTING.MAXIMUM_DRAWNDOWN_PERCENTAGE, maximumDrawndownPercentage.ToString());
//         EnvironmentVariables.VariableInjectList.TryAdd(LoggingVariables.REPORT_TO_ELASTICSEARCH, "TRUE");
//         EnvironmentVariables.VariableInjectList.TryAdd(TradingVariables.SCALING_FACTOR, "TestEnvironmentSetup,1;");

//         return new ServiceCollection()
//             .AddSingleton<ITradingObjects, TradingObjects>()
//             .BuildServiceProvider(true);
//     }

//     [Fact]
//     public void PopulateAccountObjTest(){

//         var provider = Setup(maximumDrawndownPercentage: 50);

//         var tradingObject = provider.GetService<ITradingObjects>();

//         Assert.Equal(500, tradingObject?.accountObj.openingEquity);
//         Assert.Equal(500, tradingObject?.accountObj.pnl);
//         Assert.Equal(50, tradingObject?.accountObj.maximumDrawndownPercentage);

//         tradingObject?.tradeHistory.TryAdd("1", new TradeHistoryObject(){
//             profit=10
//         });
//         tradingObject?.accountObj.AddTradeProftOrLoss(10);

//         Assert.Equal(510, tradingObject?.accountObj.pnl);
//         Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

//         tradingObject?.tradeHistory.TryAdd("2", new TradeHistoryObject(){
//             profit=0
//         });
//         tradingObject?.accountObj.AddTradeProftOrLoss(0);

//         Assert.Equal(510, tradingObject?.accountObj.pnl);
//         Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

//         tradingObject?.tradeHistory.TryAdd("3", new TradeHistoryObject(){
//             profit=100
//         });
//         tradingObject?.accountObj.AddTradeProftOrLoss(100);

//         Assert.Equal(610, tradingObject?.accountObj.pnl);
//         Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());


//         tradingObject?.tradeHistory.TryAdd("4", new TradeHistoryObject(){
//             profit=-210
//         });
//         tradingObject?.accountObj.AddTradeProftOrLoss(-210);

//         Assert.Equal(400, tradingObject?.accountObj.pnl);
//         Assert.False(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

//         tradingObject?.tradeHistory.TryAdd("5", new TradeHistoryObject(){
//             profit=-200
//         });
//         tradingObject?.accountObj.AddTradeProftOrLoss(-200);

//         Assert.Equal(200, tradingObject?.accountObj.pnl);
//         Assert.True(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

//     [Fact]
//     public void TestVariousDrawdownPercentages(){

//         var provider = Setup(maximumDrawndownPercentage: 10);

//         var tradingObject = provider.GetService<ITradingObjects>();

//         Assert.Equal(500, tradingObject?.accountObj.openingEquity);
//         Assert.Equal(500, tradingObject?.accountObj.pnl);
//         Assert.Equal(10, tradingObject?.accountObj.maximumDrawndownPercentage);

//         tradingObject?.accountObj.AddTradeProftOrLoss(-60);

//         Assert.Equal(440, tradingObject?.accountObj.pnl);
//         Assert.True(tradingObject?.accountObj.hasAccountExceededDrawdownThreshold());

//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

//     [Fact]
//     public void TestCalculateProfitBUY(){

//         var provider = Setup(maximumDrawndownPercentage: 10);

//         var tradingObject = provider.GetService<ITradingObjects>();
        

//         // Test a BUY
//         // Entry at 120 (ASK), current level 100 (BID) = -19 profit (as slippage made the price lower)
//         var currentLevel = 80m;
//         var priceObj = new PriceObj() {
//             symbol="TestEnvironmentSetup",
//             ask=100,
//         };

//         var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);
//         // Create a trade request object to open a trade
//         var scalingFactor = EnvironmentVariables.GetScalingFactor(priceObj.symbol);
//         var request = new RequestObject(priceObj, TradeDirection.BUY, key, scalingFactor) {
//             size = 1
//         };

//         var output = tradingObject?.accountObj.CalculateProfit(currentLevel, request);

//         Assert.Equal(-20,output);


//         // Test a BUY
//         // Entry at 100 (ASK), current level 120 (BID) = 20 profit
//         currentLevel = 120m;
//         priceObj = new PriceObj() {
//             symbol="TestEnvironmentSetup",
//             ask=100,
//         };


//         // Create a trade request object to open a trade
//         request = new RequestObject(priceObj, TradeDirection.BUY, key, scalingFactor) {
//             size = 1
//         };

//         output = tradingObject?.accountObj.CalculateProfit(currentLevel, request);

//         Assert.Equal(20,output);

//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

//     [Fact]
//     public void TestCalculateProfitSELL(){

//         var provider = Setup(maximumDrawndownPercentage: 10);
        

//         var tradingObject = provider.GetService<ITradingObjects>();


//         // Test a BUY
//         // Entry at 120 (bid), current level 100 (ask) = 20 profit
//         var currentLevel = 100m;
//         var priceObj = new PriceObj() {
//             symbol="TestEnvironmentSetup",
//             bid=120,
//         };

//         var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);

//         // Create a trade request object to open a trade
//         var scalingFactor = EnvironmentVariables.GetScalingFactor(priceObj.symbol);
//         var request = new RequestObject(priceObj, TradeDirection.SELL, key, scalingFactor)  {
//             size = 1
//         };

//         var output = tradingObject?.accountObj.CalculateProfit(currentLevel, request);

//         Assert.Equal(20, output);

//         // Clean
//         TestEnvironment.CleanEnvironment();

//     }
// }