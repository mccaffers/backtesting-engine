using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using Utilities;

namespace backtesting_engine;

public class TaskManager : ITaskManager
{
    protected readonly BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
    protected readonly CancellationTokenSource cts = new CancellationTokenSource();

    readonly IConsumer con;
    readonly IIngest ing;

    public TaskManager(IConsumer c, IIngest i)
    {
        this.con = c;
        this.ing = i;
    }

    public async Task IngestAndConsume()
    {
        ing.EnvironmentSetup();

        Task taskProduce = ing.ReadLines(buffer, cts.Token).CancelOnFaulted(cts);
        Task consumer = con.ConsumeAsync(buffer, cts.Token).CancelOnFaulted(cts);

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