using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using backtesting_engine;
using backtesting_engine.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace invokeStrategy;

public class Function
{

    private static EnvironmentVariables variables = new EnvironmentVariables();
    private IServiceCollection? _serviceCollection;

    public Function() => ConfigureServices();

    private void ConfigureServices()
    {
        // add dependencies here
        _serviceCollection = new ServiceCollection()
            .RegisterStrategies(variables)
            .AddSingleton<IEnvironmentVariables>(variables);
    }
    
    public string FunctionHandler(DynamoDBEvent input, ILambdaContext context)
    {
        Console.WriteLine($"Beginning to process {input.Records.Count} records...");

       foreach (var record in input.Records)
        {
            var newImage = record.Dynamodb.NewImage; // Or "OldImage" property
            var myObject = DynamoDbStreamToObject.Convert<PriceObj>(newImage);
            System.Console.WriteLine(JsonConvert.SerializeObject(myObject));
        }

        var esClient = new ElasticClient(settings);
        //Send an initial report to ElasticSearch
        var response = esClient.Index(new LambdaReport() {
                            message = "triggered",
                            date = DateTime.Now
                        }, b => b.Index("live-invoke"));

        System.Console.WriteLine(response);

        return "Success";
    }
}

public class LambdaReport {
    public string message {get;set;}
}