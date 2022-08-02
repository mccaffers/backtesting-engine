using backtesting_engine;

public interface IWebNotification
{
    Task Message(OhlcObject input, bool force = false);
    Task AccountUpdate(decimal input);
}

