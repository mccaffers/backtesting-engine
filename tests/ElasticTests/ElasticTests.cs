using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Nest;
using Reporting;
using trading_exception;
using Utilities;
using Xunit;

namespace Tests;

[Collection("Sequential")]
public class ElasticTests
{

    [Fact]
    public async void TestElasticSearchFinalReport(){

        TestEnvironment.SetEnvironmentVariables(); 
        
        Environment.SetEnvironmentVariable("reportingEnabled", "true");

        var services = new ServiceCollection()
        .AddSingleton<ITradingObjects, TradingObjects>()
        .AddSingleton<ISystemObjects, SystemObjects>().BuildServiceProvider(true);

        var elasticClient = new Mock<IElasticClient>();
        elasticClient.Setup(c => c.IndexAsync(It.IsAny<ReportFinalObj>(),
                                                It.IsAny<Func<IndexDescriptor<ReportFinalObj>,
                                                    IIndexRequest<backtesting_engine.ReportFinalObj>>>(), 
                                                It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult<IndexResponse>(new IndexResponse()));

        var elasticMock = new Mock<Elastic>(services, elasticClient.Object){
            CallBase = true
        };

        List<ReportTradeObj> tradeObjects = new List<ReportTradeObj>();
        tradeObjects.Add(new ReportTradeObj(){
            date=DateTime.Now,
            symbols=new string[]{"EURUSD", "GBPUSD"},
            pnl=200,
            runID="unittests",
            tradeProfit=10,
            runIteration=1
        });

        await elasticMock.Object.EndOfRunReport("");

        Assert.True(elasticMock.Object.switchHasSentFinalReport);
    }

    [Fact]
    public async void TestElasticStackMethod(){

        // Arrange Environment
        TestEnvironment.SetEnvironmentVariables(); 
        Environment.SetEnvironmentVariable("reportingEnabled", "true");

        // Arrange local variables
        bool indexAsyncCalled=false;
        IndexName index="";

        var services = new ServiceCollection()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>().BuildServiceProvider(true);
        
        var elasticClient = new Mock<IElasticClient>();

        // Setup the elasticClient to mock the IndexAsync Method
        elasticClient.Setup(c => c.IndexAsync<TradingException>(It.IsAny<TradingException>(),
                                        It.IsAny<Func<IndexDescriptor<TradingException>, IIndexRequest<TradingException>>>(), 
                                            It.IsAny<CancellationToken>()))
                        .Returns((TradingException exception,
                                    Func<IndexDescriptor<TradingException>, IIndexRequest<TradingException>> indexDescriptor,
                                        CancellationToken ct) => {
                                // Capture the index name, and return a blank index response
                                index = indexDescriptor.Invoke(new IndexDescriptor<TradingException>()).Index;
                                return Task.FromResult(new IndexResponse());
                        })
                        .Callback(()=>indexAsyncCalled=true);     

        var elasticMock = new Mock<Elastic>(services, elasticClient.Object){
            CallBase = true
        };

        // Act
        await elasticMock.Object.SendStack(new TradingException("test"));

        // Assert
        Assert.True(indexAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("exception", index); // check it's the right index
    }

    [Fact]
    public async void TestElasticTradeUpdatekMethod(){

        // Arrange Environment
        TestEnvironment.SetEnvironmentVariables(); 
        Environment.SetEnvironmentVariable("reportingEnabled", "true");

        // Arrange local variables
        bool bulkAsyncCalled=false;
        IndexName index="";
        int recordsToBulkIndex=0;

        string symbolName ="TestEnvironmentSetup";

        var tradingObject = new TradingObjects();

        var priceObj = new PriceObj(){
            date=DateTime.Now,
            symbol=symbolName,
            bid=100,
            ask=120
        };

        var reqObj = new RequestObject(priceObj)
        {
            direction = TradeDirection.BUY,
            size = 1,
            stopDistancePips = 20,
            limitDistancePips = 20,
        };

        tradingObject.openTrades.TryAdd(reqObj.key, reqObj);
        
        priceObj.bid = 120;
        priceObj.ask = 140;

        tradingObject.openTrades.Where(x=>x.Key == reqObj.key).First().Value.UpdateClose(priceObj);

        var provider = new ServiceCollection()
            .AddSingleton<ITradingObjects>(tradingObject)
            .AddSingleton<ISystemObjects, SystemObjects>()
            .BuildServiceProvider(true);
        
        var elasticClient = new Mock<IElasticClient>();

        // Setup the elasticClient to mock the BulkAsync Method
        elasticClient.Setup(c => c.BulkAsync(It.IsAny<Func<BulkDescriptor,IBulkRequest>>(),It.IsAny<CancellationToken>()))
                        .Returns((Func<BulkDescriptor,IBulkRequest> bulkDescriptor, CancellationToken ct) => {
                                var bulkDesc = new BulkDescriptor().IndexMany<ReportTradeObj>(new List<ReportTradeObj>());
                                var operations = bulkDescriptor.Invoke(bulkDesc).Operations;
                                index = operations.First().Index;
                                recordsToBulkIndex= operations.Count;
                                return Task.FromResult(new BulkResponse());
                        })
                        .Callback(()=>{
                            bulkAsyncCalled=true;
                        });     

        var elasticMock = new Mock<Elastic>(provider, elasticClient.Object){
            CallBase = true
        };

        // Force an trigger of the bulk update method as more than {x} seconds have passed
        elasticMock.Object.lastPostTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));

        // Act
        await elasticMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);

        // Assert
        Assert.Equal(1, recordsToBulkIndex); // one record has been added
        Assert.True(bulkAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("trades", index); // check it's the right index
    }
}