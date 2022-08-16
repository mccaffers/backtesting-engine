using backtesting_engine.interfaces;
using backtesting_engine_models;
using Newtonsoft.Json;

namespace backtesting_engine;

public class WebNotification : IWebNotification
{
    private DateTime lastSentAccount = DateTime.Now;
    public async Task AccountUpdate(decimal input)
    {

        if(DateTime.Now.Subtract(lastSentAccount).TotalMilliseconds < 200){
            return;
        }
        lastSentAccount = DateTime.Now;

        await PublishMessage("account", input.ToString());
    }

    public async Task TradeUpdate(TradeHistoryObject input)
    {
        await PublishMessage("trade", JsonConvert.SerializeObject(input));
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

        await PublishMessage("price", JsonConvert.SerializeObject(input));
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


