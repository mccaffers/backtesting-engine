using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using Utilities;

namespace backtesting_engine;

public class Main
{
    protected readonly BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();

    public async Task IngestAndConsume(IConsumer c, Ingest i)
    {
        i.EnvironmentSetup();
        
        Task taskProduce = i.ReadLines(buffer);
        Task consumer = c.ConsumeAsync(buffer);

        if (taskProduce.Exception != null)
            throw new ArgumentException("Ingest Exception", taskProduce.Exception);

        if (consumer.Exception != null)
            throw new ArgumentException("Consumer Exception", consumer.Exception);

        // await until both the producer and the consumer are finished:
        await Task.WhenAll(taskProduce, consumer);
    }
}
