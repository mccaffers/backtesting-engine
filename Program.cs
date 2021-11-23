using Utilities;

namespace backtesting_engine
{
    public class Program
    {

        private static string[] csvFolderPaths = EnvironmentVariable.Get("csvFolderPaths").Split(",");

        public static void Main(string[] args)
        {

            Console.WriteLine("Starting... ");

            foreach(var csvFolderPath in csvFolderPaths){
                DirectoryInfo di = new DirectoryInfo(csvFolderPath);
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    Console.WriteLine(file.Name);
                }
            }

        }
    }
}