namespace Utilities;

public static class ConsoleLogger {

    public static void Log(string message){

        bool consoleLog = false;
        _ = bool.TryParse(EnvironmentVariables.Get("consoleLog", true), out consoleLog);
        if(consoleLog){
            System.Console.WriteLine(message);
        }
    }

}