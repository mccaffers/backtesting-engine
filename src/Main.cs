using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using Utilities;

namespace backtesting_engine;

public class TaskManager : ITaskManager
{
    protected readonly BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
    protected readonly CancellationTokenSource cts = new CancellationTokenSource();

    IConsumer c;
    IIngest i;

    public TaskManager(IConsumer c, IIngest i)
    {
        this.c = c;
        this.i = i;
    }

    public async Task IngestAndConsume()
    {
        i.EnvironmentSetup();

        Task taskProduce = i.ReadLines(buffer, cts.Token).CancelOnFaulted(cts);
        Task consumer = c.ConsumeAsync(buffer, cts.Token).CancelOnFaulted(cts);

        // await until both the producer and the consumer are finished
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