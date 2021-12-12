using System.Globalization;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine
{
    public class Ingest
    {
        public static readonly BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        
        public async Task ProcessFiles(IEnumerable<string> fileNames)
        {

            Task taskProduce = new Input().ReadLines(fileNames);
            Task consume = ConsumeAsync();

            // await until both the producer and the consumer are finished:
            await Task.WhenAll(taskProduce, consume);
            System.Console.WriteLine("Complete");
        }

        async Task ConsumeAsync()
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
