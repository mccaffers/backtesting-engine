    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Run(async () => {
                await backtesting_engine.Program.Main(new string[1]{"web"});
            }).Wait();
        }
    }