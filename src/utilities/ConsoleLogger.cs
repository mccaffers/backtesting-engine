namespace Utilities;

public static class ConsoleLogger {

    private static readonly bool systemLog;
    private static readonly bool consoleLog;
    private static readonly bool lambdaLog;

    static ConsoleLogger () {
        _ = bool.TryParse(LoggingVariables.SYSTEM_LOG.Value(), out systemLog);
        _ = bool.TryParse(LoggingVariables.CONSOLE_LOG.Value(), out consoleLog);
        _ = bool.TryParse(LoggingVariables.LAMBDA_LOG.Value(), out lambdaLog);
    }

    public static void SystemLog(string message){
        if(systemLog){
            Console.WriteLine(message);
        }
    }

    public static void Log(string message){
        if(consoleLog){
            Console.WriteLine(message);
        }
    }

     public static void Lambda(string symbol, string message){
        if(lambdaLog){
            Console.WriteLine(symbol + " - " + message);
        }
    }

}