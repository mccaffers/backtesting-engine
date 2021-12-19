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
            await new Main().Deploy();
        }
    }



}