using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest
{
    public class Ingest
    {
        public readonly BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        
        public async Task ProcessFiles(IEnumerable<string> fileNames)
        {
            Task taskProduce = new Input().ReadLines(fileNames, buffer);
            Task consume = new Consumer().ConsumeAsync(buffer);

            // await until both the producer and the consumer are finished:
            await Task.WhenAll(taskProduce, consume);
            System.Console.WriteLine("Complete");
        }

   
       
    }
}
