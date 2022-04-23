using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace trading_exception;

[SuppressMessage("Sonar Code Smell", "S3925:ISerializable should be implemented correctly", Justification = "Extending the exception class to add a date reference, for use in elastic")]
public class TradingException : Exception {

    public DateTime date {get;set;}

    public TradingException(){}

    public TradingException(string? message) : base(message) {
        date = DateTime.Now;
    }


}