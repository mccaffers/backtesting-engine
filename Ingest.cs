using System.Globalization;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine
{
    public class Ingest
    {
        private readonly string[] epicsArary = EnvironmentVariable.Get("epics").Split(",");
        private readonly string dtFormat = "yyyy-MM-ddTHH:mm:ss.fff";
        private readonly char[] sep = ",".ToCharArray();

        private BufferBlock<PriceObj> buffer = new BufferBlock<PriceObj>();
        private Dictionary<string, PriceObj> lineBuffer = new Dictionary<string, PriceObj>();
        private DateTime currentTime = new DateTime();
        
        public async Task ProcessFiles(IEnumerable<string> fileNames)
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
                    
                    if(IsValidLine(line)){
                        var output = ReadLine(file.Key, line ?? "");
                        if(output!=null)
                            lineBuffer.Add(file.Key, output); 
                    } 
                    
                }
                await OrderDictionaryAndSendOldestToBuffer();
            }
            buffer.Complete();
        }

        async Task OrderDictionaryAndSendOldestToBuffer() {
            if(lineBuffer.Count()==0)
                return;

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
                System.Console.WriteLine(JsonConvert.SerializeObject(line));
            }

            // if we reach hear buffer has been marked Complete()
        }

        bool IsValidLine(string? line){
            
            if(line == null)
                return false;

            string[] values = line.Split(sep);

            if (values[0].Length==0 || values[1].Length==0 || 
                    values[1].Length==0 || values[0] == "UTC")  // Missing values
                return false;

            return extractDt(values[0]).parsed; // Finaly check, so pass back true if datetime parsed safely, false if not
        }

        (bool parsed, DateTime datetime) extractDt(string dtString){
            DateTime dtHolder = new DateTime();
            dtString = dtString.Substring(0, dtString.LastIndexOf("+")); // Stripping everything off before the + sign
            var parsedDt = DateTime.TryParseExact(dtString, dtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dtHolder);
            return (parsedDt, dtHolder);
        }

        PriceObj? ReadLine(string fileName, string line){
          
            string[] values = line.Split(sep);

            var bid = decimal.Parse(values[1]);
            var dateTime = extractDt(values[0]).datetime;
            var ask = decimal.Parse(values[2]);
            var epic = epicsArary.Where(x=>fileName.Contains(x)).First(); //TOOD Dangerous

            return new PriceObj(){
                        epic = epic,
                        bid = bid,
                        ask = ask,
                        date = dateTime
            };
        }
    }
}
