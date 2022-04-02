using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using backtesting_engine_models;
using Newtonsoft.Json;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_strategies;
using Reporting;
using trading_exception;
using backtesting_engine_operations;
using backtesting_engine.interfaces;

namespace backtesting_engine;

class Program
{
    async static Task Main(string[] args) =>
        await Task.FromResult(
            new ServiceCollection()
            .RegisterStrategies()
            .AddSingleton<IOpenOrder, OpenOrder>()
            .AddSingleton<ICloseOrder, CloseOrder>()
            .AddSingleton<IIngest, Ingest>()
            .AddSingleton<IConsumer, Consumer>()
            .AddSingleton<IPositions, Positions>()
            .AddSingleton<ITaskManager, TaskManager>()
            .AddSingleton<ISystemSetup, SystemSetup>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<IRequestOpenTrade, RequestOpenTrade>()
            .BuildServiceProvider(true)
            .CreateScope()
            .ServiceProvider.GetRequiredService<ISystemSetup>())
        .ContinueWith(task=>{
            System.Console.WriteLine("Finished");
        });
}


