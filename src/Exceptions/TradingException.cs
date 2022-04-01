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

    public TradingException(string? message, Exception? innerException) : base(message, innerException) {
        date = DateTime.Now;
    }

    protected TradingException(SerializationInfo info, StreamingContext context) : base(info, context) {
        date = DateTime.Now;
    }

    public TradingException(string? message, string? paramName) : base(message, paramName) {
        date = DateTime.Now;
    }

    public TradingException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException) {
        date = DateTime.Now;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context) {
        base.GetObjectData(info, context);
        info.AddValue("TradingException.Date", this.date);
    }
}