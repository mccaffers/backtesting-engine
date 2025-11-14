// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using backtesting_engine;
// using backtesting_engine.analysis;
// using backtesting_engine.interfaces;
// using backtesting_engine_models;
// using backtesting_engine_operations;
// using Microsoft.Extensions.DependencyInjection;
// using Moq;
// using Nest;
// using Utilities;
// using Xunit;

// namespace Tests;

// [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
// public class PositionTests
// {
        
//     static string symbolName="TestEnvironmentSetup";

//     public static ServiceProvider Setup(decimal accountEquity)
//     {
//         // Arrange
//         TestEnvironment.SetEnvironmentVariables(); 

//         EnvironmentVariables.VariableInjectList.TryAdd(BACKTESTING.ACCOUNT_EQUITY, accountEquity.ToString());
//         EnvironmentVariables.VariableInjectList.TryAdd(BACKTESTING.REPORT_INDIVIDUAL_TRADES, "TRUE");
//         EnvironmentVariables.VariableInjectList.TryAdd(LoggingVariables.REPORT_TO_ELASTICSEARCH, "TRUE");
//         EnvironmentVariables.VariableInjectList.TryAdd(TradingVariables.SCALING_FACTOR, "TestEnvironmentSetup,1;");

//         // Setup local dependency provider
//         return new ServiceCollection()
//             .AddSingleton<ITradingObjects, TradingObjects>()
//             .AddSingleton<ISystemObjects, SystemObjects>()
//             .AddSingleton<IElasticClient>(new Mock<IElasticClient>().Object)
//             .AddSingleton<IReporting, Reporting>()
//             .AddSingleton<ICloseOrder, CloseOrder>()
//             .AddSingleton<IOpenOrder, OpenOrder>()
//             .AddSingleton<IPositions, Positions>()
//             .BuildServiceProvider(true);
//     }

//     public static IEnumerable<object[]> Data =>
//     new List<object[]>
//     {
//         // fields: ask, bid, account equity, direction
//         new object[] {120m, 150m, 100m, "BUY"}, // positive test
//         new object[] {100m, 50m, 200m, "BUY"}, // different account opening
//         new object[] {100m, 20m, 1000m, "BUY"}, // large initial account
//         new object[] {100m, 200m, 100m, "BUY"}, // negative test
//         new object[] {100m, 200m, 100m, "SELL"}, // positive test
//         new object[] {200m, 100m, 100m, "SELL"}, // negative test
//     };

//     [Theory]
//     [MemberData(nameof(Data))]
//     public void BuyCalculationsTest(decimal ask, decimal bid, decimal accountEquity, string direction) {

//         TradeDirection tradeDirection = (TradeDirection)Enum.Parse(typeof(TradeDirection), direction);
 
//         var provider = Setup(accountEquity);
//         var tradingObject = provider.GetService<ITradingObjects>();
//         var positions = provider.GetService<IPositions>();
//         var openOrder = provider.GetService<IOpenOrder>();

//         // Act
//         // Inital price event
//         var priceObj = new PriceObj() {
//             symbol=symbolName,
//             ask=ask,
//             bid=bid,
//         };

//         var key = DictionaryKeyStrings.OpenTrade(priceObj.symbol, priceObj.date);
//         var scalingFactor = EnvironmentVariables.GetScalingFactor(priceObj.symbol);

//         // Create a trade request object to open a trade
//         openOrder?.Request(new RequestObject(priceObj,
//                                              tradeDirection,
//                                              key,
//                                              scalingFactor) {
//             size = 1,
//         });

//         // Create a new price event
//         var priceObjNext = new PriceObj(){
//             symbol=symbolName,
//             bid=bid,
//             ask=ask
//         };

//         positions?.Review(priceObjNext);

//         // There is now an order excution delay on closing trades
//         priceObjNext.date=priceObjNext.date.AddSeconds(61);
//         // positions?.PushRequests(priceObjNext);

//         // var slippage = 1m / envVariables?.GetScalingFactor(symbolName);
//         var slippage = 0;

//         var expectedPnL = accountEquity - ( (ask - slippage) - bid);
//         if(tradeDirection == TradeDirection.SELL){
//             expectedPnL = ( (bid+slippage) - ask ) + accountEquity;
//         }

//         // Assert
//         Assert.Equal(1, tradingObject?.tradeHistory.Count); // one record has been added
//         Assert.Equal(expectedPnL, tradingObject?.accountObj.pnl); // one record has been added

//         // Clean
//         TestEnvironment.CleanEnvironment();
//     }

    
// }