using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_models;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Utilities;

namespace invokeStrategy;

public class LiveRequestOpenTrade : IRequestOpenTrade
{
    readonly IOpenOrder openOrder;


    public LiveRequestOpenTrade(IOpenOrder openOrder)
    {
        System.Console.WriteLine("Made LiveRequestOpenTrade");
        this.openOrder = openOrder;
    }

    public async void Request(RequestObject reqObj)
    {

        System.Console.WriteLine("Received trade request");
        System.Console.WriteLine(JsonConvert.SerializeObject(reqObj));

        // Check DynamoDB for open trades
        var result = await PerformCRUDOperations(reqObj.priceObj, reqObj.direction);

        if(result){
            var tradeOpenNotification = new LambdaReport() {
                function = "invokeStrategy",
                priceObj = reqObj.priceObj,
                date = DateTime.Now
            };

            var response = Function.esClient.Index(tradeOpenNotification, b => b.Index("live-trade-open-notification"));
            if (!response.IsValid)
{
                // Handle errors
                System.Console.WriteLine(response.DebugInformation);
                System.Console.WriteLine(response.ServerError.Error);
            }
            System.Console.WriteLine("Opening trade for " + JsonConvert.SerializeObject(tradeOpenNotification));
        } else {
            System.Console.WriteLine("Trade already open");
        }

    }

    public async Task<bool> PerformCRUDOperations(PriceObj priceObj, TradeDirection direction)
    {

        var item = await Function.dynamoDbContext.LoadAsync<TradingPositions>(priceObj.symbol);

        System.Console.WriteLine(JsonConvert.SerializeObject(item));
        if(item != null){
            // back out, a trade is already open
            return false;
        }

        TradingPositions newPosition = new TradingPositions
        {
            symbol = priceObj.symbol,
            pnl = 0,
            direction = direction.ToString()
        };

        System.Console.WriteLine(JsonConvert.SerializeObject(newPosition));

        // Save the book to the ProductCatalog table.

        await Function.dynamoDbContext.SaveAsync<TradingPositions>(newPosition);
        // await context.SaveAsync(newPosition);
        return true;
    }
}

public class TradingPositions {
    public string symbol {get;set;} = string.Empty;
    public decimal pnl {get;set;}
    public string direction {get;set;}
}