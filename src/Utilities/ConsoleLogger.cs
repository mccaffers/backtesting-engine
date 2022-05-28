namespace Utilities;

public static class ConsoleLogger {

    private static readonly bool systemLog;
    private static readonly bool consoleLog;

    static ConsoleLogger () {
        _ = bool.TryParse(EnvironmentVariables.Get("systemLog", true), out systemLog);
        _ = bool.TryParse(EnvironmentVariables.Get("consoleLog", true), out consoleLog);
    }

    public static void SystemLog(string message){
        if(systemLog){
            System.Console.WriteLine(message);
        }
    }

    public static void Log(string message){
        if(consoleLog){
            System.Console.WriteLine(message);
        }
    }

}