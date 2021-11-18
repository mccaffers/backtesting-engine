using Utilities;

namespace backtesting_engine
{
    public class Program
    {

        private static string[] csvFolderPaths = EnvironmentVariable.Get("csvFolderPaths").Split(",");

        public static void Main(string[] args)
        {

            System.Console.WriteLine("Local feeder starting... ");

            foreach(var csvFolderPath in csvFolderPaths){
                DirectoryInfo di = new DirectoryInfo(csvFolderPath);
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    System.Console.WriteLine(file.Name);
                }
            }

        }
    }
}