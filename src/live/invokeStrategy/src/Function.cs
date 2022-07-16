using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using backtesting_engine;
using backtesting_engine.interfaces;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace invokeStrategy;

public class Function
{
    private static EnvironmentVariables variables = new EnvironmentVariables();

    private static AmazonDynamoDBConfig ClientConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.EUWest1 };        
    private static CloudConnectionPool pool = new CloudConnectionPool(variables.elasticCloudID, new BasicAuthenticationCredentials(variables.elasticUser, variables.elasticPassword));
    private static ConnectionSettings settings = new ConnectionSettings(pool).RequestTimeout(TimeSpan.FromMinutes(2)).EnableApiVersioningHeader().EnableDebugMode();

    private ServiceProvider? serviceProvider;

    public Function() => ConfigureServices();

    private void ConfigureServices()
    {
        // add dependencies here
        serviceProvider = new ServiceCollection()
            .RegisterStrategies(variables)
            .AddTransient<IRequestOpenTrade, LiveRequestOpenTrade>()
            .AddTransient<IOpenOrder, OpenOrder>()
            .AddSingleton<IEnvironmentVariables>(variables)
            .BuildServiceProvider();

    }

    public static DynamoDBContext dynamoDbContext = new DynamoDBContext(new AmazonDynamoDBClient(ClientConfig));
    public static ElasticClient esClient = new ElasticClient(settings);
    
    public string FunctionHandler(DynamoDBEvent input, ILambdaContext context)
    {   

        System.Console.WriteLine(JsonConvert.SerializeObject(input));

        Console.WriteLine($"Beginning to process {input.Records.Count} records...");

       foreach (var record in input.Records)
        {
            var newImage = record.Dynamodb.NewImage; // Or "OldImage" property
            var myObject = DynamoDbStreamToObject.Convert<PriceObj>(newImage);
            System.Console.WriteLine(JsonConvert.SerializeObject(myObject));
        }

        // Get the newest DynamoDB object
        var dynamoDBTick = input.Records.OrderBy(x=>DynamoDbStreamToObject.Convert<PriceObj>(x.Dynamodb.NewImage).date).Last();
        var priceObj = DynamoDbStreamToObject.Convert<PriceObj>(dynamoDBTick.Dynamodb.NewImage);

        //Send an initial report to ElasticSearch
        var response = esClient.Index(new LambdaReport() {
                            function = "invokeStrategy",
                            priceObj = priceObj,
                            strategy = variables.strategy,
                            date = DateTime.Now
                        }, b => b.Index("live-invoke"));

        var strategy = serviceProvider?.GetRequiredService<IStrategy>();
        strategy.Invoke(priceObj);
        
        return "Success";
    }
}

public class LambdaReport {
    public string function {get;set;} = string.Empty;
    public PriceObj? priceObj {get;set;}
    public string strategy {get;set;} = string.Empty;
    public DateTime date {get;set;}
    
}