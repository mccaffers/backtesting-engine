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
using backtesting_engine_operations;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using trading_exception;
using Utilities;
using Xunit;

namespace Tests;

public class ReportingTests
{

    [Fact (Skip = "To fix")]
    public async Task TestElasticSearchFinalReport(){

        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        environmentMock.SetupGet<bool>(x=>x.reportingEnabled).Returns(true);

        var provider = new ServiceCollection()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IEnvironmentVariables>(environmentMock.Object)
            .BuildServiceProvider(true);

        var elasticClient = new Mock<IElasticClient>();
        elasticClient.Setup(c => c.IndexAsync(It.IsAny<ReportFinalObj>(),
                                                It.IsAny<Func<IndexDescriptor<ReportFinalObj>,
                                                    IIndexRequest<ReportFinalObj>>>(), 
                                                It.IsAny<CancellationToken>()))
                                .ReturnsAsync( () => {return new Mock<IndexResponse>().Object;});

        var reportingMock = new Mock<Reporting>(provider, elasticClient.Object, environmentMock.Object){
            CallBase = true
        };

        reportingMock.Object.EndOfRunReport("");
        ConsoleLogger.Log(reportingMock.Object.switchHasSentFinalReport.ToString());
        Assert.True(reportingMock.Object.switchHasSentFinalReport);
    }

    [Fact]
    public async Task TestElasticStackMethod(){


        // Arrange Environment Variables
        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        environmentMock.SetupGet<bool>(x=>x.reportingEnabled).Returns(true);
        var environmentObj = environmentMock.Object;
        
        // Arrange local variables
        bool indexAsyncCalled=false;
        IndexName index=string.Empty;

        // Setup local dependency provider
        var services = new ServiceCollection()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<IEnvironmentVariables>(environmentObj)
            .AddSingleton<ISystemObjects, SystemObjects>().BuildServiceProvider(true);
        
        // Setup the elasticClient to mock the IndexAsync Method
        var elasticClient = new Mock<IElasticClient>();
        elasticClient.Setup(c => c.IndexAsync<TradingException>(It.IsAny<TradingException>(),
                                        It.IsAny<Func<IndexDescriptor<TradingException>, IIndexRequest<TradingException>>>(), 
                                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TradingException exception,
                                    Func<IndexDescriptor<TradingException>, IIndexRequest<TradingException>> indexDescriptor,
                                        CancellationToken ct) => {
                                        index = indexDescriptor.Invoke(new IndexDescriptor<TradingException>()).Index; // Capture elastic index name
                                        return new Mock<IndexResponse>().Object; // Fake any response from Elastic
                    })
                    .Callback(()=>indexAsyncCalled=true);     

        var reportingMock = new Mock<Reporting>(services, elasticClient.Object, environmentObj){
            CallBase = true
        };

        // Act
        await reportingMock.Object.SendStack(new TradingException("test"));

        // Assert
        Assert.True(indexAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("exception", index); // check it's the right index
    }

    [Fact (Skip = "To fix")]
    public async Task TestElasticTradeUpdatekMethod(){

        // Arrange Environment Variables
        var environmentMock = TestEnvironment.SetEnvironmentVariables(); 
        environmentMock.SetupGet<bool>(x=>x.reportingEnabled).Returns(true);
        var environmentObj = environmentMock.Object;

        // Arrange local variables
        bool bulkAsyncCalled=false;
        int recordsToBulkIndex=0;
        IndexName index=string.Empty;

        // Setup local dependency provider
        var provider = new ServiceCollection()
            .AddSingleton<IEnvironmentVariables>(environmentObj)
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .BuildServiceProvider(true);

        // Setup the elasticClient to mock the BulkAsync Method
        var elasticClient = new Mock<IElasticClient>();
        elasticClient.Setup(c => c.BulkAsync(It.IsAny<Func<BulkDescriptor,IBulkRequest>>(),It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Func<BulkDescriptor,IBulkRequest> bulkDescriptor, CancellationToken ct) => {

                                // Create a BulkDescriptor to run the Func against so I can retrieve the operations
                                var bulkDesc = new BulkDescriptor().IndexMany<ReportTradeObj>(new List<ReportTradeObj>());

                                // Invoke on the bulk descriptor
                                var operations = bulkDescriptor.Invoke(bulkDesc).Operations;

                                // Retrieve the index name and count
                                index = operations.First().Index;
                                recordsToBulkIndex= operations.Count;

                                // Return a blank bulk response
                                return new Mock<BulkResponse>().Object;
                        })
                        .Callback(()=>{
                            // TODO I think I can do this via a verify as well
                            bulkAsyncCalled=true; // Record that bulkAsync has been called
                        });     

        var reportingMock = new Mock<Reporting>(provider, elasticClient.Object, environmentMock.Object){
            CallBase = true
        };

        string symbolName="TestEnvironmentSetup";

        //
        // Act 1 - Should have 1 trade to update elastic with
        ///

        recordsToBulkIndex=0;
        bulkAsyncCalled=false;
        index=string.Empty;

        reportingMock.Object.lastPostTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        reportingMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);

        // Assert
        Assert.Equal(1, recordsToBulkIndex); // one record has been added
        Assert.True(bulkAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("trades", index); // check it's the right index

        //
        // Act 2 - Should have 4 trades to update elastic with
        //

        // Reset variables
        recordsToBulkIndex=0;
        bulkAsyncCalled=false;
        index=string.Empty;

        reportingMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);
        reportingMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);
        reportingMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);

        // Must update the time prior to the last update so the batch update is triggered
        reportingMock.Object.lastPostTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        reportingMock.Object.TradeUpdate(DateTime.Now, symbolName, 10);

        // Assert
        Assert.Equal(4, recordsToBulkIndex); // one record has been added
        Assert.True(bulkAsyncCalled); // Confirm that the indexAsync was called
        Assert.Equal("trades", index); // check it's the right index
    }


}