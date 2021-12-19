using System.Collections.Immutable;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new Run().Setup();
        }
    }

    public class Run
    {
        
        private ImmutableArray<string> symbols { get; set; }
        private string? folderPath { get; set; }
        protected readonly BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        private List<string> fileNames { get; set; } = new List<string> ();

        public async Task Setup()
        {
            this.EnvironmentSetup();
            await this.IngestAndConsume(new Consumer(), new Ingest(symbols, fileNames));
        }

        public void EnvironmentSetup() {

            var env = EnvironmentVariables.Get("symbols").Split(",");

            symbols = ImmutableArray.Create(env);
            folderPath = EnvironmentVariables.Get("folderPath");

            // Create a temporary list
            var arrayHolder = new List<string>();

            // Loop around every epic to check what files are present
            foreach(var epic in this.symbols){
                DirectoryInfo di = new DirectoryInfo(Path.Combine(this.folderPath, epic));
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    arrayHolder.Add(file.FullName);
                }
            }
            
            fileNames = arrayHolder.OrderBy(x=>x).ToList();
            
        }

        public async Task IngestAndConsume(IConsumer c, Ingest i){

            Task taskProduce = i.ReadLines(buffer);
            Task consumer = c.ConsumeAsync(buffer);

            if(taskProduce.Exception !=null ){
                throw new Exception("Ingest failed", taskProduce.Exception);
            }

            if(consumer.Exception !=null ){
                throw new Exception("Consumer failed", consumer.Exception);
            }

            // await until both the producer and the consumer are finished:
            await Task.WhenAll(taskProduce, consumer).ContinueWith((t) => {
                if (t.IsFaulted) {
                    Environment.FailFast("Exception", t.Exception);
                }
                if (t.IsCompleted) {

                }
            });
        }

    }

}