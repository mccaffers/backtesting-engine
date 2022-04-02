using System.Threading.Tasks.Dataflow;

namespace backtesting_engine;

public interface IConsumer
{
    Task ConsumeAsync(BufferBlock<PriceObj> buffer, CancellationToken cts);
}