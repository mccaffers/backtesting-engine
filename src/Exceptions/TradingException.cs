using System.Runtime.Serialization;
using System.Security.Permissions;

namespace trading_exception;

[Serializable]
// Important: This attribute is NOT inherited from Exception, and MUST be specified 
// otherwise serialization will fail with a SerializationException stating that
// "Type X in Assembly Y is not marked as serializable."
public class TradingException : ArgumentException {

    public DateTime date {get;set;}

    public TradingException(){}

    public TradingException(string? message) : base(message) {
        date = DateTime.Now;
    }


}