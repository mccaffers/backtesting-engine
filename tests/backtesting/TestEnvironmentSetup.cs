using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Tests;
using Utilities;
using Xunit;
using static Tests.EnvironmentVariableTests;

namespace Tests;

class TestEnvironment {

    public static string folderPath {get;set;} = PathUtil.GetTestPath("TestEnvironmentSetup");
    public static string[] fileNames {get;set;} = new string[]{"testSymbol.csv"};

    public static void SetEnvironmentVariables(){

        Environment.SetEnvironmentVariable("consoleLog", "false");

        EnvironmentJson? source; 
        using (StreamReader r = new StreamReader(Path.Combine(PathUtil.GetTestPath("environmentVariables.json"))))
        {  
            string json = r.ReadToEnd();  
            source = JsonSerializer.Deserialize<EnvironmentJson>(json);  
        }  

        if(source != null){
            EnvironmentVariables.Inject(TradingVariables.STRATEGY, source.STRATEGY);
            EnvironmentVariables.Inject(TradingVariables.SCALING_FACRTOR, source.SCALING_FACRTOR);
            EnvironmentVariables.Inject(TradingVariables.STOP_DISTANCE_IN_PIPS, source.STOP_DISTANCE_IN_PIPS);
            EnvironmentVariables.Inject(TradingVariables.LIMIT_DISTANCE_IN_PIPS, source.LIMIT_DISTANCE_IN_PIPS);
            EnvironmentVariables.Inject(TradingVariables.TRAILING_STOP_LOSS_ACTIVE, source.TRAILING_STOP_LOSS_ACTIVE);
            EnvironmentVariables.Inject(TradingVariables.TRAILING_STOP_LOSS_VALUE, source.TRAILING_STOP_LOSS_VALUE);
            EnvironmentVariables.Inject(TradingVariables.MOVING_LIMIT_VALUE, source.MOVING_LIMIT_VALUE);
            EnvironmentVariables.Inject(TradingVariables.TRADING_SIZE, source.TRADING_SIZE);

            EnvironmentVariables.Inject(BACKTESTING.SYMBOL_FOLDER, source.SYMBOL_FOLDER);
            EnvironmentVariables.Inject(BACKTESTING.SYMBOLS, source.SYMBOLS);
            EnvironmentVariables.Inject(BACKTESTING.RUN_ID, source.RUN_ID);
            EnvironmentVariables.Inject(BACKTESTING.TICK_DATA_FOLDER, source.TICK_DATA_FOLDER);
            EnvironmentVariables.Inject(BACKTESTING.ACCOUNT_EQUITY, source.ACCOUNT_EQUITY);
            EnvironmentVariables.Inject(BACKTESTING.MAXIMUM_DRAWNDOWN_PERCENTAGE, source.MAXIMUM_DRAWNDOWN_PERCENTAGE);
            EnvironmentVariables.Inject(BACKTESTING.S3_BUCKET, source.S3_BUCKET);
            EnvironmentVariables.Inject(BACKTESTING.S3_PATH, source.S3_PATH);
            EnvironmentVariables.Inject(BACKTESTING.RUN_ITERATION, source.RUN_ITERATION);
            EnvironmentVariables.Inject(BACKTESTING.FASTER_PROCESSING_BY_SKIPPING_SOME_DATA, source.FASTER_PROCESSING_BY_SKIPPING_SOME_DATA);
            EnvironmentVariables.Inject(BACKTESTING.INSTANCE_COUNT, source.INSTANCE_COUNT);

            EnvironmentVariables.Inject(LoggingVariables.REPORT_TO_ELASTICSEARCH, source.REPORT_TO_ELASTICSEARCH);
            EnvironmentVariables.Inject(LoggingVariables.SYSTEM_LOG, source.SYSTEM_LOG);
            EnvironmentVariables.Inject(LoggingVariables.CONSOLE_LOG, source.CONSOLE_LOG);
            EnvironmentVariables.Inject(LoggingVariables.LAMBDA_LOG, source.LAMBDA_LOG);
            
            EnvironmentVariables.Inject(ELASTICSEARCH.ELASTIC_USER, source.ELASTIC_USER);
            EnvironmentVariables.Inject(ELASTICSEARCH.ELASTIC_PASSWORD, source.ELASTIC_PASSWORD);
            EnvironmentVariables.Inject(ELASTICSEARCH.CLOUD_ID, source.CLOUD_ID);

            EnvironmentVariables.Inject(StrategyVariables.VARIABLE_A, source.VARIABLE_A);
            EnvironmentVariables.Inject(StrategyVariables.VARIABLE_B, source.VARIABLE_B);
            EnvironmentVariables.Inject(StrategyVariables.VARIABLE_C, source.VARIABLE_C);
            EnvironmentVariables.Inject(StrategyVariables.VARIABLE_D, source.VARIABLE_D);
        }
    }

    public static void CleanEnvironment(){

        EnvironmentVariables.VariableInjectList.Clear();
 
        Environment.SetEnvironmentVariable(TradingVariables.STRATEGY.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.SCALING_FACRTOR.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.STOP_DISTANCE_IN_PIPS.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.LIMIT_DISTANCE_IN_PIPS.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.TRAILING_STOP_LOSS_ACTIVE.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.TRAILING_STOP_LOSS_VALUE.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.MOVING_LIMIT_VALUE.ToString(), null);
        Environment.SetEnvironmentVariable(TradingVariables.TRADING_SIZE.ToString(), null);

        Environment.SetEnvironmentVariable(BACKTESTING.SYMBOL_FOLDER.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.SYMBOLS.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.RUN_ID.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.TICK_DATA_FOLDER.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.ACCOUNT_EQUITY.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.MAXIMUM_DRAWNDOWN_PERCENTAGE.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.S3_BUCKET.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.S3_PATH.ToString(), null);
        // Environment.SetEnvironmentVariable(BACKTESTING.YEAR_START.ToString(), null);
        // Environment.SetEnvironmentVariable(BACKTESTING.YEAR_END.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.RUN_ITERATION.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.FASTER_PROCESSING_BY_SKIPPING_SOME_DATA.ToString(), null);
        Environment.SetEnvironmentVariable(BACKTESTING.INSTANCE_COUNT.ToString(), null);

        Environment.SetEnvironmentVariable(LoggingVariables.REPORT_TO_ELASTICSEARCH.ToString(), null);
        
        Environment.SetEnvironmentVariable(ELASTICSEARCH.ELASTIC_USER.ToString(), null);
        Environment.SetEnvironmentVariable(ELASTICSEARCH.ELASTIC_PASSWORD.ToString(), null);
        Environment.SetEnvironmentVariable(ELASTICSEARCH.CLOUD_ID.ToString(), null);

        Environment.SetEnvironmentVariable(StrategyVariables.VARIABLE_A.ToString(), null);
        Environment.SetEnvironmentVariable(StrategyVariables.VARIABLE_B.ToString(), null);
        Environment.SetEnvironmentVariable(StrategyVariables.VARIABLE_C.ToString(), null);
        Environment.SetEnvironmentVariable(StrategyVariables.VARIABLE_D.ToString(), null);
    

    }
}