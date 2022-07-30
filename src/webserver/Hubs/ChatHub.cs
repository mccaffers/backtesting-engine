using Microsoft.AspNetCore.SignalR;
using Webserver.Api.Hubs.Clients;

namespace Webserver.Api.Hubs
{
    public class ChatHub : Hub<IChatClient>
    { }
}