using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

public class DynamoDbStreamToObject
{
    private static readonly AmazonDynamoDBConfig ClientConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.EUWest1 };
    private static readonly DynamoDBContext DynamoDbContext = new DynamoDBContext(new AmazonDynamoDBClient(ClientConfig));

    public static T Convert<T>(Dictionary<string, AttributeValue> dynamoDbImage) 
    {
        var dynamoDocument = Document.FromAttributeMap(dynamoDbImage);
        return DynamoDbContext.FromDocument<T>(dynamoDocument);
    }
}