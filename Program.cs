namespace backtesting_engine
{
    public class Program
    {
        public static void Main(string[] args)
        {

            System.Console.WriteLine("Local feeder starting... ");

            for (var i = 0; i < args.Length; i++)
            {
                DirectoryInfo di = new DirectoryInfo(args[i]);
                var files = di.GetFiles("*.csv").OrderBy(x => x.Name);

                foreach (var file in files) {
                    System.Console.WriteLine(file.Name);
                }
            }

        }
    }
}