namespace backtesting_engine_models;

public class StrategyDefinition
{
    public required string UUID { get; set; }
    public bool Trade { get; set; } = true;
    public required TRADING_VARIABLES_CLASS TRADING_VARIABLES { get; set; }
    public OHLC_RSI_VARIABLES_CLASS? OHLC_RSI_VARIABLES { get; set; }
    public OHLC_ALL_IN_VARIABLES_CLASS? OHLC_ALL_IN_VARIABLES { get; set; }

    public class OHLC_RSI_VARIABLES_CLASS
    {
        public required string RSI_LONG { get; set; }
        public required string RSI_SHORT { get; set; }
    }

    public class OHLC_ALL_IN_VARIABLES_CLASS
    {
        public required string RSI_LONG { get; set; }
        public required string RSI_SHORT { get; set; }
    }

    public class TRADING_VARIABLES_CLASS
    {
        public required string STRATEGY { get; set; }
        public required string STOP_DISTANCE_IN_PIPS { get; set; }
        public required string LIMIT_DISTANCE_IN_PIPS { get; set; }
        public string TRAILING_STOP_LOSS_ACTIVE { get; set; } = "0";
        public string TRAILING_STOP_LOSS_VALUE { get; set; } = "0";
        public string MOVING_LIMIT_VALUE { get; set; } = "0";
        public required string TRADING_SIZE { get; set; }
    }
    
}
