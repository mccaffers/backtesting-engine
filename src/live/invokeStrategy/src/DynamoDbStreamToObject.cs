using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace invokeStrategy;

public class DynamoDbStreamToObject
{

    public static T Convert<T>(Dictionary<string, AttributeValue> dynamoDbImage) 
    {
        var dynamoDocument = Document.FromAttributeMap(dynamoDbImage);
        return Function.dynamoDbContext.FromDocument<T>(dynamoDocument);
    }
}