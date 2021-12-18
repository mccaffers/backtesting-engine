using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using Utilities;

namespace backtesting_engine_ingest
{
    public interface IInput
    {
        Task ReadLines(IEnumerable<string> fileNames, BufferBlock<PriceObj> buffer);
    }

    public class Input : IInput
    {
        private static readonly string dtFormat = "yyyy-MM-ddTHH:mm:ss.fff";
        private static readonly char[] sep = ",".ToCharArray();

        protected Dictionary<string, StreamReader> streamDictionary = new Dictionary<string, StreamReader>();
        private readonly Dictionary<string, PriceObj> localInputBuffer = new Dictionary<string, PriceObj>();

        // 1. Builds a dictionary of filenames (.csv's) to read
        // 2. Populates a local buffer with the read line and compare timestamps
        // 3. Closes all streamreader files at the end
        public async Task ReadLines(IEnumerable<string> fileNames, BufferBlock<PriceObj> buffer)
        {
            foreach (var file in fileNames)
            {
                streamDictionary.Add(file, ReadFile(file));
            }

            await LoopStreamDictionaryAndReadLine(streamDictionary, buffer);
            StreamReaderCleanup();
        }

        protected virtual void StreamReaderCleanup()
        {
            foreach (var file in streamDictionary)
            {
                file.Value.Close(); // Close Streamreader
                file.Value.Dispose(); // Dispose
                streamDictionary.Remove(file.Key);
            }
        }

        protected virtual StreamReader ReadFile(string file)
        {
            return new StreamReader(file);
        }

        // 1. Ensure every file isn't at the end of it's stream
        // 2. Check each file individually and remove filename from the dictionary if so
        // 3. Check if the local buffer already has this filename (symbol), if not populate and add to local buffer
        // 4. Review all the symbols on the buffer, and select the oldest, loop and repeat
        protected virtual async Task LoopStreamDictionaryAndReadLine(Dictionary<string, StreamReader> streamDictionary, BufferBlock<PriceObj> buffer)
        {

            // Loop statements if files still have contents to parse
            while (streamDictionary.Any(x => !x.Value.EndOfStream))
            {
                foreach (var file in streamDictionary)
                {

                    if (file.Value.EndOfStream)
                    { // if any of the files ever finish early
                        streamDictionary.Remove(file.Key);
                        continue;
                    }

                    if (!localInputBuffer.ContainsKey(file.Key))
                    { //need to ignore file if it already exists in lineBuffer dictionary
                        string line = await file.Value.ReadLineAsync() ?? "";
                        PopulateLocalBuffer(file.Key, line);
                    }

                }
                await GetOldestItemOffBuffer(buffer);
            }
            buffer.Complete();
        }

        // Expecting data in the following format
        //  UTC,AskPrice,BidPrice,AskVolume,BidVolume
        //  2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75
        void PopulateLocalBuffer(string fileName, string line)
        {

            string[] values = line.Split(sep);

            // Initial scan to confirm the line has the right values
            if (!ArrayHasRightValues(values))
            {
                return;
            }

            var bid = decimal.Parse(values[1]);
            var dtExtract = extractDt(values[0]);

            if (!dtExtract.parsed)
            {
                return;
            }

            var dateTime = dtExtract.datetime;
            var ask = decimal.Parse(values[2]);
            var epic = EnvironmentVariables.symbols.Where(x => fileName.Contains(x)).First();

            localInputBuffer.Add(fileName, new PriceObj()
            {
                epic = epic,
                bid = bid,
                ask = ask,
                date = dateTime
            });
        }

        // Send the oldest line to the Ingest Buffer
        async Task GetOldestItemOffBuffer(BufferBlock<PriceObj> buffer)
        {
            if (!localInputBuffer.Any())
                return;

            var orderedDictionary = localInputBuffer.OrderBy(x => x.Value.date);
            var oldestElementKey = orderedDictionary.First().Key;
            await buffer.SendAsync(localInputBuffer[oldestElementKey]);
            localInputBuffer.Remove(oldestElementKey);
        }

        // Simple value and length check on the line
        public static bool ArrayHasRightValues(string[] values)
        {
           return values.Length > 4 &&
                values.Any(x => x.Length > 0) &&
                values.Skip(1).All(x=> decimal.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out _));
        }

        // Extract the datetime from the string
        public static (bool parsed, DateTime datetime) extractDt(string dtString)
        {
            DateTime localDt;
            if(dtString.Contains('+')){
                dtString = dtString.Substring(0, dtString.LastIndexOf("+")); // Stripping everything off before the + sign
            }
            var parsedDt = DateTime.TryParseExact(dtString, dtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out localDt);
            return (parsedDt, localDt);
        }
    }
}