using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine.analysis;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using trading_exception;
using Utilities;

namespace backtesting_engine;

public class TaskManager : ITaskManager
{
    public BufferBlock<PriceObj> buffer { get; init; } 
    protected readonly CancellationTokenSource cts = new CancellationTokenSource();

    readonly IConsumer con;
    readonly IIngest ing;
    readonly IReporting reporting;
    readonly IEnvironmentVariables envVariables;

    public TaskManager(IConsumer c, IIngest i, IReporting reporting, IEnvironmentVariables envVariables)
    {
        this.con = c;
        this.ing = i;
        this.buffer = new BufferBlock<PriceObj>();
        this.reporting = reporting;
        this.envVariables = envVariables;
    }

    public async Task IngestAndConsume()
    {
        ing.EnvironmentSetup();

        Task taskProduce = Task.Run(() => ing.ReadLines(buffer, cts.Token)).CancelOnFaulted(cts);
        Task consumer =  Task.Run(() => con.ConsumeAsync(buffer, cts.Token)).CancelOnFaulted(cts);

        await Task.WhenAll(taskProduce, consumer);
    }
}

public static class ExtensionMethods
{
    public static Task CancelOnFaulted(this Task task, CancellationTokenSource cts)
    {
        task.ContinueWith(task => cts.Cancel(), cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        return task;
    }
}