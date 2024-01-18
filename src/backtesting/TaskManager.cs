using System.Threading.Tasks.Dataflow;
using backtesting_engine.interfaces;
using backtesting_engine_ingest;
using trading_exception;

namespace backtesting_engine;

public class TaskManager : ITaskManager
{
    public BufferBlock<PriceObj> buffer { get; init; } 
    protected readonly CancellationTokenSource cts = new CancellationTokenSource();

    readonly IConsumer con;
    readonly IIngest ing;
    readonly IEnvironmentVariables envVaribles;

    public TaskManager(IConsumer c, IIngest i, IEnvironmentVariables env)
    {
        this.con = c;
        this.ing = i;
        this.envVaribles = env;
        this.buffer = new BufferBlock<PriceObj>();
    }

    public async Task IngestAndConsume()
    {
        ing.EnvironmentSetup();

        // Updated to ensure that a stack trace is pushed up the stack
        // if either task fails

        Task taskProduce = Task.Run(() => ing.ReadLines(buffer, cts.Token))
            .CancelOnFaulted(cts)
            .ContinueWith(task => {
                if(task.IsFaulted){
                    cts.Cancel();
                    buffer.SendAsync(new PriceObj());
                    
                    
                     if(task.Exception?.InnerException is not TradingException) {
                        throw new Exception (task.Exception?.Message, task.Exception);
                    } 
                }
            });

        Task consumer = Task.Run(() => con.ConsumeAsync(buffer, cts.Token)).CancelOnFaulted(cts)
           .ContinueWith(task => {
                if(task.IsFaulted){
                    cts.Cancel();
                    
                    if(task.Exception?.InnerException is TradingException) {
                        throw new TradingException(task.Exception?.InnerException.Message, "", envVaribles);
                    } else {
                        throw new Exception (task.Exception?.Message, task.Exception);
                    }

                }
            });

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