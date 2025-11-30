using backtesting_engine_models;

public class BacktestingLoggingVariables
{
    public bool CONSOLE_LOG { get; set; } = false;
    public bool LAMBDA_LOG { get; set; } = false;
    public bool SYSTEM_LOG { get; set; } = false;
}

public class BacktestingVariables : BacktestingLoggingVariables
{
    public required string RUN_ID { get; set; }
    public int RUN_ITERATION { get; set; } = 1;
    public int MAXIMUM_DRAWNDOWN_PERCENTAGE { get; set; } = 5;
    public int MAX_MONTHLY_DRAWDOWN_THRESHOLD { get; set; } = 5;
    public int MAX_OVERALL_DRAWDOWN_THRESHOLD { get; set; } = 5;
    public bool REPORT_INDIVIDUAL_TRADES { get; set; } = false;
    public bool REPORT_TO_ELASTICSEARCH { get; set; } = false;
    public bool REPORT_LOSSES { get; set; } = false;
    public required string SYMBOLS { get; set; }
    public int ACCOUNT_EQUITY { get; set; } = 10000;
    public bool FASTER_PROCESSING_BY_SKIPPING_SOME_DATA { get; set; } = true;
    public double TRADING_SIZE { get; set; } = 1;
    public required int LAST_MONTHS { get; set; }
    public DateTime DATE_TO { get; set; } = DateTime.MinValue;
    public bool TRAILING_STOP_LOSS_ACTIVE { get; set; } = false;
    public int TRAILING_STOP_LOSS_VALUE { get; set; } = 0;
    public decimal MIN_TRADE_RATIO { get; set; } = decimal.Zero;
    public string ELASTICSEARCH_URI { get; set; } = string.Empty;
    public string ELASTICSEARCH_USERNAME { get; set; } = string.Empty;
    public string ELASTICSEARCH_PASSWORD { get; set; } = string.Empty;
    public required StrategyDefinition STRATEGY { get; set; }
}
