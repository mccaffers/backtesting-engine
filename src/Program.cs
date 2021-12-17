using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine_ingest;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting... ");

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

            // pass all the file names CLEANUP
            await new Ingest().ProcessFiles(arrayHolder.OrderBy(x=>x)); //get oldest date first

        }

    }

    

    
}