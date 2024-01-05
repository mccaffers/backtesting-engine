using System.Collections.Immutable;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest;

public interface IIngest
{
    List<string> fileNames { get; }
    Dictionary<string, StreamReader> streamDictionary { get; }
    Dictionary<string, PriceObj> localInputBuffer { get; }

    void EnvironmentSetup();
    Task ReadLines(BufferBlock<PriceObj> buffer, CancellationToken cts);
}

public class Ingest : IIngest
{
    private readonly IEnumerable<string> symbols;

    public virtual List<string> fileNames { get; } = new List<string>();
    public Dictionary<string, StreamReader> streamDictionary { get; }
    public Dictionary<string, PriceObj> localInputBuffer { get; }
    readonly IEnvironmentVariables envVariables;

    public Ingest(IEnvironmentVariables envVariables)
    {
        this.envVariables = envVariables;
        this.symbols = envVariables.symbols;
        this.streamDictionary = new Dictionary<string, StreamReader>();
        this.localInputBuffer = new Dictionary<string, PriceObj>();
    }

    public virtual void EnvironmentSetup()
    {
        // Create a temporary list
        var arrayHolder = new List<string>();

        // Loop around every epic to check what files are present
        foreach (var symbol in this.symbols)
        {
            var symbolFolder = Path.Combine(envVariables.tickDataFolder, symbol);

            DirectoryInfo di = new DirectoryInfo(symbolFolder);
            var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

            if(files.Count() > 1){
                throw new Exception("Data is pulled on a yearly basis, remove extra years from /tick folder");
            }

            foreach (var file in files)
            {
                arrayHolder.Add(file.FullName);
            }
        }

        this.fileNames.AddRange(arrayHolder.OrderBy(x => x).ToList());
    }

    // 1. Builds a dictionary of filenames (.csv's) to read
    // 2. Populates a local buffer with the read line and compare timestamps
    // 3. Closes all streamreader files at the end
    CancellationToken _cts;
    public async Task ReadLines(BufferBlock<PriceObj> buffer, CancellationToken cts)
    {
        _cts = cts;
        
        foreach (var file in fileNames)
        {
            // change this file string to 
            var symbol = this.symbols.First(x => file.Contains(x));
            streamDictionary.Add(symbol, ReadFile(file));
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
        var streamOutput = new StreamReader(file);
        return streamOutput;
    }

    // 1. Ensure every file isn't at the end of it's stream
    // 2. Check each file individually and remove filename from the dictionary if so
    // 3. Check if the local buffer already has this filename (symbol), if not populate and add to local buffer
    // 4. Review all the symbols on the buffer, and select the oldest, loop and repeat
    protected async virtual Task LoopStreamDictionaryAndReadLine(Dictionary<string, StreamReader> streamDictionary, BufferBlock<PriceObj> buffer)
    {
        // Loop statements if files still have contents to parse
        while (streamDictionary.Any(x => !x.Value.EndOfStream))
        {

            _cts.ThrowIfCancellationRequested();
            
            // To prevent RAM being maxed out and slowing the system down
            // limit the buffer to hold 10,000 ticks at any one point
            if(buffer.Count>10000){ 
                continue;
            }

            foreach (var file in streamDictionary)
            {

                // Check if any of the files ever finish early
                if (file.Value.EndOfStream)
                { 
                    streamDictionary.Remove(file.Key);
                    continue;
                }

                // Ignore file if it already exists in lineBuffer dictionary
                if (!localInputBuffer.ContainsKey(file.Key))
                { 
                    string line = file.Value.ReadLine() ?? "";
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
    public virtual void PopulateLocalBuffer(string key, string line)
    {
        string[] values = line.Split(',');

        // Initial scan to confirm the line has the right values
        if (!ArrayHasRightValues(values))
            return;

        var dtExtract = extractDt(values[0]);
        // var future = new DateTime(2015, 1, 15, 9, 31, 1);

        // if(dtExtract.datetime < future ){
        //     return;
        // }

        if (!dtExtract.parsed)
            return;

        var ask = DecimalUtil.ParseDecimal(values[1]);
        var bid = DecimalUtil.ParseDecimal(values[2]);

        localInputBuffer.Add(key, new PriceObj()
        {
            symbol = key,
            bid = bid,
            ask = ask,
            date = dtExtract.datetime
        });

    }

    // Send the oldest line to the Ingest Buffer
    public async Task GetOldestItemOffBuffer(BufferBlock<PriceObj> buffer)
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
            values.Skip(1).All(x => char.IsDigit(x.FirstOrDefault('a'))); 
            // just a random default 'a' so that the application doesn't throw on an empty column eg. ,,,,
    }

    // Extract the datetime from the string
    public static (bool parsed, DateTime datetime) extractDt(string dtString)
    {
        DateTime localDt;
        if (dtString.Contains('+')){
            dtString = dtString.Substring(0, dtString.LastIndexOf("+")); // Stripping everything off before the + sign
        }
        var parsedDt = DateTime.TryParseExact(dtString, StringFormats.dtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out localDt);
        return (parsedDt, localDt);
    }
}
