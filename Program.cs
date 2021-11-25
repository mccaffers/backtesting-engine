using System.Globalization;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine
{
    
    public class Program
    {

        private static string _env_csvFolderPaths = EnvironmentVariable.Get("csvFolderPaths");
        private static string[] _env_epics = EnvironmentVariable.Get("epics").Split(",");

        private BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        private static DateTime currentTime = new DateTime();

        private Dictionary<string, PriceObj> lineBuffer = new Dictionary<string, PriceObj>();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting... ");

            // Create a temporary list
            var arrayHolder = new List<string>();

            // Loop around every epic to check what files are present
            foreach(var epic in _env_epics){
                DirectoryInfo di = new DirectoryInfo(_env_csvFolderPaths + "/" + epic);
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    arrayHolder.Add(file.FullName);
                }
            }

            // pass all the file names CLEANUP
            var p = new Program();
            await p.ProcessFiles(arrayHolder.OrderBy(x=>x)); //get oldest date first

        }

        async Task ProcessFiles(IEnumerable<string> fileNames)
        {

            Task taskProduce = ReadLines(fileNames);
            Task consume = ConsumeAsync();

            // await until both the producer and the consumer are finished:
            await Task.WhenAll(taskProduce, consume);
            System.Console.WriteLine("Complete");
        }

        async Task ReadLines(IEnumerable<string> fileNames)
        {

            Dictionary<string, StreamReader> streamDictionary = new Dictionary<string, StreamReader>();

            foreach(var file in fileNames){
                streamDictionary.Add(file, new StreamReader(file));
            }

            await PopulateDictionary(streamDictionary);

            foreach(var file in streamDictionary){ 
                file.Value.Close(); // Close Streamreader
                file.Value.Dispose(); // Dispose
                streamDictionary.Remove(file.Key);
            }

        }

        async Task PopulateDictionary(Dictionary<string, StreamReader> streamDictionary){
          
            while(streamDictionary.Any(x=>!x.Value.EndOfStream)){       
                
                foreach(var file in streamDictionary){ 

                    if(file.Value.EndOfStream){ // if any of the files ever finish early
                        System.Console.WriteLine("Finishe with " + file.Key);
                        streamDictionary.Remove(file.Key);
                        continue;
                    }

                    if(lineBuffer.ContainsKey(file.Key)){ //need to ignore file if it already exists in lineBuffer dictionary
                        continue;
                    }

                    string? line = await file.Value.ReadLineAsync();
                    line = await file.Value.ReadLineAsync();

                    var output = ReadLine(file.Key, line);
                    if(output!=null){
                        lineBuffer.Add(file.Key, output); //save it as soon as read
                    }
                }
    
                await OrderDictionaryAndSendOldestToBuffer();
            }
            buffer.Complete();
        }

        async Task OrderDictionaryAndSendOldestToBuffer() {
            var orderedDictionary = lineBuffer.OrderBy(x=>x.Value.date);
            var oldKey = orderedDictionary.First().Key;
            await buffer.SendAsync(lineBuffer[oldKey]);
            lineBuffer.Remove(oldKey);
        }

        async Task ConsumeAsync()
        {
            while (await buffer.OutputAvailableAsync())
            {
                var line = await buffer.ReceiveAsync();
                System.Console.WriteLine(line.epic + " " + line.date);
            }

            // if we reach hear buffer has been marked Complete()
        }

        private string dtFormat = "yyyy-MM-ddTHH:mm:ss.fff";
        private string sep = ",";

        PriceObj? ReadLine(string fileName, string? line){
            if(line==null){
                return null;
            }
            string[] values = line.Split(sep.ToCharArray());
            if (values[0].Length==0 || values[1].Length==0 || values[1].Length==0 || values[0] == "UTC")
            {
                return null;
            }

            var bid = decimal.Parse(values[1]);
            var dateTime = values[0];
            var ask = decimal.Parse(values[2]);
            var dtHolder = new DateTime();

            try {
                // Remove the last . off the time, for some reason the MT5 data has another minisecond
                dateTime = dateTime.Substring(0, dateTime.LastIndexOf("+"));
                dtHolder = DateTime.ParseExact(dateTime, dtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            } 
            catch (Exception ex) { 
                System.Console.WriteLine("Time Issue");
            }

            var epic = _env_epics.Where(x=>fileName.Contains(x)).First(); //TOOD Dangerous

            return new PriceObj(){
                        epic = epic,
                        bid = bid,
                        ask = ask,
                        date = dtHolder
                    };
        }
    }

    public class PriceObj {
        public string epic {get;set;}
        public decimal bid {get;set;}
        public decimal ask {get;set;}
        public DateTime date {get; set;}
        public int scalingFactor {get;set;}
        public string currencyCode {get;set;}
    }

    
}