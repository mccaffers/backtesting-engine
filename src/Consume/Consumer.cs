using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest;

public interface IConsumer
{
    Task ConsumeAsync(BufferBlock<PriceObj> buffer);
}

public class Consumer : IConsumer
{

    public static ConcurrentDictionary<string, OpenTradeObject> openTrades = new ConcurrentDictionary<string, OpenTradeObject>();

    public async Task ConsumeAsync(BufferBlock<PriceObj> buffer)
    {
        while (await buffer.OutputAvailableAsync())
        {
            var line = await buffer.ReceiveAsync();
            System.Console.WriteLine(JsonConvert.SerializeObject(line));


            // Testing
            // Wait for so many event
            // Open trade randomly
            openTrades.TryAdd("key", RequestOpenTrade.Request(new RequestObject(){
                value = line.ask
            }));
        }

        // if we reach here the buffer has been marked Complete()
    }


}
