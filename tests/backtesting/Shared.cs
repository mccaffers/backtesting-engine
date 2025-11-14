using System;
using System.IO;
using System.Reflection;

namespace Tests;

public static class ReflectionExtensions {
    public static T GetFieldValue<T>(this object obj, string name) {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        if (field?.GetValue(obj) is T myValue)
            return myValue;
       
        return default!;
    }
    
}

public static class PathUtil {

    public static string GetTestPath(string relativePath)
    {
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath) ?? "";

        System.Console.WriteLine(dirPath);

        if(relativePath.Length > 0){
            return Path.Combine(dirPath, "Resources", relativePath);
        } else {
            return Path.Combine(dirPath, "Resources");
        }
    }
}

 public class EnvironmentJson  
{  

    // TradingVariables
    public required string STRATEGY { get; set; }  
    public required string SCALING_FACRTOR  { get; set; }  
    public required string STOP_DISTANCE_IN_PIPS { get; set; }  
    public required string LIMIT_DISTANCE_IN_PIPS  { get; set; }  
    public required string TRAILING_STOP_LOSS_ACTIVE { get; set; }  
    public required string TRAILING_STOP_LOSS_VALUE  { get; set; }  
    public required string MOVING_LIMIT_VALUE  { get; set; }  
    public required string TRADING_SIZE  { get; set; }  
    public required string SYMBOLS  { get; set; }  
    public required string SYMBOL_FOLDER  { get; set; }  
    public required string RUN_ID  { get; set; }  
    public required string ELASTIC_USER  { get; set; }  
    public required string ELASTIC_PASSWORD  { get; set; }  
    public required string CLOUD_ID  { get; set; }  
    public required string TICK_DATA_FOLDER  { get; set; }  
    public required string ACCOUNT_EQUITY  { get; set; }  
    public required string MAXIMUM_DRAWNDOWN_PERCENTAGE  { get; set; }  
    public required string S3_BUCKET  { get; set; }  
    public required string S3_PATH  { get; set; }  
    public required string YEAR_START  { get; set; }  
    public required string YEAR_END  { get; set; }  
    public required string REPORT_TO_ELASTICSEARCH  { get; set; }  
    public required string LAMBDA_LOG  { get; set; }  
    public required string SYSTEM_LOG  { get; set; }  
    public required string CONSOLE_LOG  { get; set; }  

    public required string RUN_ITERATION  { get; set; }  
    public required string FASTER_PROCESSING_BY_SKIPPING_SOME_DATA  { get; set; }  
    public required string INSTANCE_COUNT {get;set;}
    public required string VARIABLE_A {get;set;}
    public required string VARIABLE_B {get;set;}
    public required string VARIABLE_C {get;set;}
    public required string VARIABLE_D {get;set;}
    
}  