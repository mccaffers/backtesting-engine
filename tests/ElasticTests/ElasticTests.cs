using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using Reporting;
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
        elasticClient.Setup(c => c.IndexAsync(It.IsAny<ReportFinalObj>(), It.IsAny<Func<IndexDescriptor<ReportFinalObj>,IIndexRequest<backtesting_engine.ReportFinalObj>>>(), It.IsAny<CancellationToken>()))
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

        // elasticMock.Setup<Task<BulkResponse>>(x=>x.elasticWrapper(It.IsAny<List<ReportTradeObj>>(), It.IsAny<string>()))
        //         .ReturnsAsync<BulkResponse>(x=>{
        //             return Task.FromResult<BulkResponse>(It.IsAny<BulkResponse>());
        //         })

        await elasticMock.Object.EndOfRunReport("");

        // elasticMock.Object.lastPostTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        // elasticMock.Object.TradeUpdate(DateTime.Now, "test", 20);

        Assert.True(elasticMock.Object.switchHasSentFinalReport);
    }

}