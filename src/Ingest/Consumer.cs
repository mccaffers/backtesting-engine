using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest
{
    public static class Consumer
    {
        public static async Task ConsumeAsync(BufferBlock<PriceObj> buffer)
        {
            while (await buffer.OutputAvailableAsync())
            {
                var line = await buffer.ReceiveAsync();
                System.Console.WriteLine(JsonConvert.SerializeObject(line));
            }

            // if we reach hear buffer has been marked Complete()
        }


    }
}