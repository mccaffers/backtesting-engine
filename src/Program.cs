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
            var p = new Setup();
            p.EnvironmentSetup();
            await p.IngestAndConsume(new Consumer(), new Ingest());
        }
    }

    public class Setup
    {

        protected BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        public List<string> fileNames { get; set; } = new List<string> ();

        public virtual void EnvironmentSetup() {
            // Create a temporary list
            var arrayHolder = new List<string>();

            // Loop around every epic to check what files are present
            foreach(var epic in EnvironmentVariables.symbols){
                DirectoryInfo di = new DirectoryInfo(Path.Combine(EnvironmentVariables.folderPath, epic));
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    arrayHolder.Add(file.FullName);
                }
            }
            
            fileNames = arrayHolder.OrderBy(x=>x).ToList();
        }

        public async Task IngestAndConsume(IConsumer c, Ingest i){

            Task taskProduce = i.ReadLines(fileNames, buffer);
            Task consumer = c.ConsumeAsync(buffer);

            // await until both the producer and the consumer are finished:
            await Task.WhenAll(taskProduce, consumer);
        }

    }

}