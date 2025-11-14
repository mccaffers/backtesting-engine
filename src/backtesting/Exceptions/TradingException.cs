using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Utilities;

namespace trading_exception;

[SuppressMessage("Sonar Code Smell", "S3925:ISerializable should be implemented correctly", Justification = "Extending the exception class to add a date reference, for use in elastic")]
public class TradingException : Exception {

    public DateTime date {get;set;}
    public string hostname {get;set;}
    public string symbols {get;set;}
    public string runID {get;set;}
    public string runIteration {get;set;}
    public string stacktrace {get;set;} = string.Empty;

    public TradingException(string? message, string? stacktrace) : base(message) {
        date = DateTime.Now;
        hostname = EnvironmentVariables.Hostname();
        symbols = JsonConvert.SerializeObject(BACKTESTING.SYMBOLS.Value());
        runID = BACKTESTING.RUN_ID.Value();
        runIteration = BACKTESTING.RUN_ITERATION.Value();
        stacktrace= stacktrace ?? "empty";
    }

    public TradingException(string? message, Exception? innerException) : base(message, innerException)
    {
        date = DateTime.Now;
        hostname = EnvironmentVariables.Hostname();
        symbols = JsonConvert.SerializeObject(BACKTESTING.SYMBOLS.Value());
        runID = BACKTESTING.RUN_ID.Value();
        runIteration = BACKTESTING.RUN_ITERATION.Value();
        stacktrace= stacktrace ?? "empty";
    }
}