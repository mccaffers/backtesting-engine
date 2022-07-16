using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using backtesting_engine;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ingest;

public class Function
{
    
    private static AmazonDynamoDBClient dynamoDBclient = new AmazonDynamoDBClient();
    private static DynamoDBContext dynamoDBContext = new DynamoDBContext(dynamoDBclient);

    public string FunctionHandler(PriceObj input, ILambdaContext context)
    {

        var marketObj = new Document();
        marketObj["bid"] = input.bid;
        marketObj["ask"] = input.ask;
        marketObj["date"] = input.date;
        marketObj["symbol"] = input.symbol;

        Table LiveDataTable = Table.LoadTable(dynamoDBclient, "MarketDataLive"); 
        LiveDataTable.PutItemAsync(marketObj).GetAwaiter().GetResult(); 

        return JsonConvert.SerializeObject(input);
    }
}
