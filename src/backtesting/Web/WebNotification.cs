using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;

namespace backtesting_engine;

public class WebNotification : IWebNotification
{
    private DateTime lastSentAccount = DateTime.Now;
    private DateTime lastSentOpenTrade = DateTime.Now;
    private Dictionary<string,List<string>> groupMessage = new Dictionary<string,List<string>>();

    public async Task AccountUpdate(decimal input)
    {

        // if(DateTime.Now.Subtract(lastSentAccount).TotalMilliseconds < 500){
        //     return;
        // }
        // lastSentAccount = DateTime.Now;

        // await PublishMessage("account", input.ToString());

        if(!groupMessage.ContainsKey("accountUpdate")){
            groupMessage["accountUpdate"] = new List<string>();
            groupMessage["accountUpdate"].Add("");
        }

        groupMessage["accountUpdate"][0] = input.ToString();

        await Task.CompletedTask;
    }

    private Dictionary<string, RequestObject> tradeArray=new Dictionary<string, RequestObject>();

    public async Task OpenTrades(RequestObject input)
    {

        // if(DateTime.Now.Subtract(lastSentOpenTrade).TotalMilliseconds < 500){
        //     return;
        // }
        // lastSentOpenTrade = DateTime.Now;

        tradeArray.Add(input.key, input);

        // await PublishMessage("openTrades", JsonConvert.SerializeObject(input));
    }

    public async Task TradeUpdate(TradeHistoryObject input)
    {

        if(!groupMessage.ContainsKey("trade")){
            groupMessage["trade"] = new List<string>();
        }
        groupMessage["trade"].Add(JsonConvert.SerializeObject(input));

        // await PublishMessage("trade", JsonConvert.SerializeObject(input));
    }
    
    private decimal lastClose = decimal.Zero;
    private DateTime lastSent = DateTime.Now;

    public async Task PriceUpdate(OhlcObject input, bool force = false)
    {

        if(DateTime.Now.Subtract(lastSent).TotalMilliseconds < 200 && !force){
            return;
        }
        lastSent=DateTime.Now;

        if(lastClose == input.close && !force){
            return;
        }
        lastClose=input.close;

        if(!groupMessage.ContainsKey("priceUpdate")){
            groupMessage["priceUpdate"] = new List<string>();
            groupMessage["priceUpdate"].Add("");
        }
        groupMessage["priceUpdate"][0]=JsonConvert.SerializeObject(input);


        if(!groupMessage.ContainsKey("openTrades")){
            groupMessage["openTrades"] = new List<string>();
        }

        foreach(var item in tradeArray){
            groupMessage["openTrades"].Add(JsonConvert.SerializeObject(item.Value));    
        }

        await PublishMessage("price", JsonConvert.SerializeObject(groupMessage));

        // Clean up
        groupMessage = new Dictionary<string, List<string>>();
        tradeArray = new Dictionary<string, RequestObject>();
    }

    private async Task PublishMessage(string activity, string content){
        try {
            if(Webserver.Api.Program.hubContext!=null){
                await Webserver.Api.Program.hubContext.Clients.All.ReceiveMessage(new Webserver.Api.Models.ChatMessage(){
                    Activity=activity,
                    Content=content
                });
            }
        }
        catch(Exception hubEx){
            System.Console.WriteLine(hubEx);
        }
    }

}


