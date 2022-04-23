using System.Runtime.Serialization;
using System.Security.Permissions;

namespace trading_exception;

public class TradingException : Exception {

    public DateTime date {get;set;}

    public TradingException(){}

    public TradingException(string? message) : base(message) {
        date = DateTime.Now;
    }


}