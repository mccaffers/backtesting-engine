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
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using trading_exception;
using Xunit;

namespace Tests;

[Collection("Sequential")]
public class ReportingTests
{

    private void ElasticSetup(){
        TestEnvironment.SetEnvironmentVariables(); 
        Environment.SetEnvironmentVariable("reportingEnabled", "true");
    }

    private string symbolName="TestEnvironmentSetup";

    [Fact]
    public async void TestElasticSearchFinalReport(){

        ElasticSetup();

        var services = new ServiceCollection()
        .AddSingleton<ITradingObjects, TradingObjects>()
        .AddSingleton<ISystemObjects, SystemObjects>().BuildServiceProvider(true);

        var response = new Mock<IndexResponse>();

        var elasticClient = new Mock<IElasticClient>();

        elasticClient.Setup(c => c.IndexAsync(It.IsAny<ReportFinalObj>(),
                                                It.IsAny<Func<IndexDescriptor<ReportFinalObj>,
                                                    IIndexRequest<backtesting_engine.ReportFinalObj>>>(), 
                                                It.IsAny<CancellationToken>()))
                        .ReturnsAsync(response.Object);

        var reportingMock = new Mock<Reporting>(services, elasticClient.Object){
            CallBase = true
        };

        await reportingMock.Object.EndOfRunReport("");

        Assert.True(reportingMock.Object.switchHasSentFinalReport);
    }

    [Fact]
    public async void TestElasticStackMethod(){

        ElasticSetup();

        // Arrange Environment
        TestEnvironment.SetEnvironmentVariables(); 
        Environment.SetEnvironmentVariable("reportingEnabled", "true");

        // Arrange local variables
        bool indexAsyncCalled=false;
        IndexName index="";
        var response = new Mock<IndexResponse>();

        var services = new ServiceCollection()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>().BuildServiceProvider(true);
        
        var elasticClient = new Mock<IElasticClient>();

        // Setup the elasticClient to mock the IndexAsync Method
        elasticClient.Setup(c => c.IndexAsync<TradingException>(It.IsAny<TradingException>(),
                                        It.IsAny<Func<IndexDescriptor<TradingException>, IIndexRequest<TradingException>>>(), 
                                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync((TradingException exception,
                                    Func<IndexDescriptor<TradingException>, IIndexRequest<TradingException>> indexDescriptor,
                                        CancellationToken ct) => {
                                // Capture the index name, and return a blank index response
                                index = indexDescriptor.Invoke(new IndexDescriptor<TradingException>()).Index;
                                return response.Object;
                        })
                        .Callback(()=>indexAsyncCalled=true);     

        var reportingMock = new Mock<Reporting>(services, elasticClient.Object){
            CallBase = true
        };

        // Act
        await reportingMock.Object.SendStack(new TradingException("test"));

        // Assert
        Assert.True(indexAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("exception", index); // check it's the right index
    }

    [Fact]
    public async void TestElasticTradeUpdatekMethod(){

        ElasticSetup();

        // Arrange Environment
        TestEnvironment.SetEnvironmentVariables(); 
        Environment.SetEnvironmentVariable("reportingEnabled", "true");

        // Arrange local variables
        bool bulkAsyncCalled=false;
        IndexName index="";
        int recordsToBulkIndex=0;

        var response = new Mock<BulkResponse>();

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
                        .ReturnsAsync((Func<BulkDescriptor,IBulkRequest> bulkDescriptor, CancellationToken ct) => {
                                var bulkDesc = new BulkDescriptor().IndexMany<ReportTradeObj>(new List<ReportTradeObj>());
                                var operations = bulkDescriptor.Invoke(bulkDesc).Operations;
                                index = operations.First().Index;
                                recordsToBulkIndex= operations.Count;
                                return response.Object;
                        })
                        .Callback(()=>{
                            bulkAsyncCalled=true;
                        });     

        var reportingMock = new Mock<Reporting>(provider, elasticClient.Object){
            CallBase = true
        };

        // Force an trigger of the bulk update method as more than {x} seconds have passed
        reportingMock.Object.lastPostTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));

        // Act
        await reportingMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);

        // Assert
        Assert.Equal(1, recordsToBulkIndex); // one record has been added
        Assert.True(bulkAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("trades", index); // check it's the right index
    }
}