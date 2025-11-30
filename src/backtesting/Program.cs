using backtesting_engine_ingest;
using Utilities;
using Microsoft.Extensions.DependencyInjection;
using backtesting_engine_operations;
using backtesting_engine.interfaces;
using Nest;
using backtesting_engine.analysis;
using System.Text.Json;
using backtesting_engine_models;
using System.Net;

namespace backtesting_engine;

public static class Program
{
    static readonly Uri elasticUri = new(Get("ELASTICSEARCH_URI"));
    static readonly ConnectionSettings settings = new ConnectionSettings(elasticUri)
        .DefaultFieldNameInferrer(p => p)
        .RequestTimeout(TimeSpan.FromMinutes(2))
        .BasicAuthentication(Get("ELASTICSEARCH_USERNAME"), Get("ELASTICSEARCH_PASSWORD"));
        

    public static string Get(string envName, bool optional = false)
    {
        var BACKTESTING_VARIABLES = Environment.GetEnvironmentVariable(envName);
        
        if (!string.IsNullOrEmpty(BACKTESTING_VARIABLES))
            return BACKTESTING_VARIABLES;
            
        if (!optional)
            throw new ArgumentException($"Missing environment variable '{envName}'", nameof(envName));
            
        return string.Empty;
    }

    public static StrategyDefinition? BACKTESTING_STRATEGY_DEFINITION;
    public static BacktestingVariables? BACKTESTING_VARIABLES;
    
    public async static Task Main(string[] args) {

        byte[] data = Convert.FromBase64String(Get("payload"));
        string decodedString = System.Text.Encoding.UTF8.GetString(data);
        BACKTESTING_VARIABLES = JsonSerializer.Deserialize<BacktestingVariables>(decodedString) ?? throw new Exception("Missing payload");

        EnvironmentVariables.Inject(SYSTEM.ENVIRONMENT, "backtesting");

        EnvironmentVariables.Inject(TradingVariables.STRATEGY, BACKTESTING_VARIABLES.STRATEGY.TRADING_VARIABLES.STRATEGY);
        EnvironmentVariables.Inject(TradingVariables.STOP_DISTANCE_IN_PIPS, BACKTESTING_VARIABLES.STRATEGY.TRADING_VARIABLES.STOP_DISTANCE_IN_PIPS);
        EnvironmentVariables.Inject(TradingVariables.LIMIT_DISTANCE_IN_PIPS, BACKTESTING_VARIABLES.STRATEGY.TRADING_VARIABLES.LIMIT_DISTANCE_IN_PIPS);
        EnvironmentVariables.Inject(TradingVariables.TRAILING_STOP_LOSS_ACTIVE, BACKTESTING_VARIABLES.STRATEGY.TRADING_VARIABLES.TRAILING_STOP_LOSS_ACTIVE);
        EnvironmentVariables.Inject(TradingVariables.TRAILING_STOP_LOSS_VALUE, BACKTESTING_VARIABLES.STRATEGY.TRADING_VARIABLES.TRAILING_STOP_LOSS_VALUE);
        EnvironmentVariables.Inject(TradingVariables.MOVING_LIMIT_VALUE, BACKTESTING_VARIABLES.STRATEGY.TRADING_VARIABLES.MOVING_LIMIT_VALUE);

        EnvironmentVariables.Inject(BACKTESTING.SYMBOLS, BACKTESTING_VARIABLES.SYMBOLS);
        EnvironmentVariables.Inject(BACKTESTING.ACCOUNT_EQUITY, BACKTESTING_VARIABLES.ACCOUNT_EQUITY.ToString());
        EnvironmentVariables.Inject(BACKTESTING.MAXIMUM_DRAWNDOWN_PERCENTAGE, BACKTESTING_VARIABLES.MAXIMUM_DRAWNDOWN_PERCENTAGE.ToString());
        EnvironmentVariables.Inject(BACKTESTING.MAX_OVERALL_DRAWDOWN_THRESHOLD, BACKTESTING_VARIABLES.MAX_OVERALL_DRAWDOWN_THRESHOLD.ToString());
        EnvironmentVariables.Inject(BACKTESTING.MAX_MONTHLY_DRAWDOWN_THRESHOLD, BACKTESTING_VARIABLES.MAX_MONTHLY_DRAWDOWN_THRESHOLD.ToString());
        EnvironmentVariables.Inject(BACKTESTING.RUN_ITERATION, BACKTESTING_VARIABLES.RUN_ITERATION.ToString());
        EnvironmentVariables.Inject(BACKTESTING.RUN_ID, BACKTESTING_VARIABLES.RUN_ID.ToString());
        EnvironmentVariables.Inject(BACKTESTING.LAST_MONTHS, BACKTESTING_VARIABLES.LAST_MONTHS.ToString());
        EnvironmentVariables.Inject(BACKTESTING.REPORT_INDIVIDUAL_TRADES, BACKTESTING_VARIABLES.REPORT_INDIVIDUAL_TRADES.ToString());
        EnvironmentVariables.Inject(BACKTESTING.REPORT_LOSSES, BACKTESTING_VARIABLES.REPORT_LOSSES.ToString());
        EnvironmentVariables.Inject(BACKTESTING.MIN_TRADE_RATIO, BACKTESTING_VARIABLES.MIN_TRADE_RATIO.ToString());

        EnvironmentVariables.Inject(BACKTESTING.DATE_TO, BACKTESTING_VARIABLES.DATE_TO.ToString());

        EnvironmentVariables.Inject(LoggingVariables.REPORT_TO_ELASTICSEARCH, BACKTESTING_VARIABLES.REPORT_TO_ELASTICSEARCH.ToString());
        EnvironmentVariables.Inject(LoggingVariables.CONSOLE_LOG, BACKTESTING_VARIABLES.CONSOLE_LOG.ToString());
        EnvironmentVariables.Inject(LoggingVariables.LAMBDA_LOG, BACKTESTING_VARIABLES.LAMBDA_LOG.ToString());
        EnvironmentVariables.Inject(LoggingVariables.SYSTEM_LOG, BACKTESTING_VARIABLES.SYSTEM_LOG.ToString());

        EnvironmentVariables.Inject(TradingVariables.SCALING_FACTOR, "AUDUSD,10000;EURUSD,10000;GBRIDXGBP,1;GBPUSD,10000;NZDUSD,10000;USDJPY,100;GBPJPY,100;EURJPY,100;USDCAD,10000;FRAIDXEUR,1;EURGBP,10000;USA500IDXUSD,1;AUSIDXAUD,1;USDCHF,10000;XAUUSD,1;XAGUSD,1;USATECHIDXUSD,1;EURCHF,10000;DEUIDXEUR,1;USA30IDXUSD,1;LIGHTCMDUSD,1;JPNIDXJPY,1;BRENTCMDUSD,1;");

        BACKTESTING_STRATEGY_DEFINITION = BACKTESTING_VARIABLES.STRATEGY;
  
        IServiceCollection? serviceCollection= new ServiceCollection()
            .RegisterStrategies()
            .AddSingleton<IElasticClient>(provider =>
            {
                var esClient = new ElasticClient(settings);
                
                _ = Task.Run(async () =>
                {
                    var environmentReport = ReportEnvironmentVariables.Get();
                    if (BACKTESTING_STRATEGY_DEFINITION is not null)
                    {
                        environmentReport.Add("STRATEGYDEF", BACKTESTING_STRATEGY_DEFINITION);
                    }
                    environmentReport.Add("SYMBOLS", BACKTESTING.SYMBOLS.Value());
                    environmentReport.Add("RUN_ID", BACKTESTING.RUN_ID.Value());
                    environmentReport.Add("HOSTNAME", Dns.GetHostName());
                    environmentReport.Add("LAST_MONTHS", BACKTESTING.LAST_MONTHS.Value());
                    
                    await esClient.IndexAsync(environmentReport, b => b.Index("init"));
                });
                
                return esClient;
            })
            .AddSingleton<IOpenOrder, OpenOrder>()
            .AddSingleton<ICloseOrder, CloseOrder>()
            .AddSingleton<IIngest, IngestFromQuestDB>()
            .AddSingleton<IPositions, Positions>()
            .AddSingleton<IReporting, Reporting>()
            .AddSingleton<IStrategyObjects, StrategyObjects>()
            .AddSingleton<IRequestOpenTrade, RequestOpenTrade>()
            .AddSingleton<ITradingObjects, TradingObjects>()
            .AddSingleton<ISystemObjects, SystemObjects>()
            .AddSingleton<IOpenTrades, BacktestingOpenTrades>();

            using var serviceProvider = serviceCollection.BuildServiceProvider(true);
            using var scope = serviceProvider.CreateScope();
            var ingest = scope.ServiceProvider.GetRequiredService<IIngest>();
        
            // Now explicitly run the heavy operation
            if (ingest is IngestFromQuestDB questDbIngest)
            {
                await questDbIngest.ExecuteAsync();
            }

    }
}


