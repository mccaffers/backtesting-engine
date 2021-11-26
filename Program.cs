using System.Globalization;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine
{
    
    public class Program
    {

        private static readonly string CSVFolderPaths = EnvironmentVariable.Get("csvFolderPaths");
        private static readonly string[] epicArray = EnvironmentVariable.Get("epics").Split(",");

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting... ");

            // Create a temporary list
            var arrayHolder = new List<string>();

            // Loop around every epic to check what files are present
            foreach(var epic in epicArray){
                DirectoryInfo di = new DirectoryInfo(CSVFolderPaths + "/" + epic);
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    arrayHolder.Add(file.FullName);
                }
            }

            // pass all the file names CLEANUP
            var p = new Ingest();
            await p.ProcessFiles(arrayHolder.OrderBy(x=>x)); //get oldest date first

        }

    }

    

    
}