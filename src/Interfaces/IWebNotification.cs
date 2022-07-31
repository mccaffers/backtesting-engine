using backtesting_engine;

public interface IWebNotification
{
    Task Message(OhlcObject input);
    Task AccountUpdate(decimal input);
}

