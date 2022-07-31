using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Webserver.Api.Hubs;
using Webserver.Api.Hubs.Clients;
using Webserver.Api.Models;

namespace Webserver.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        public readonly IHubContext<ChatHub, IChatClient> _chatHub;
        public ChatController(IHubContext<ChatHub, IChatClient> chatHub)
        {
            _chatHub = chatHub;
        }

        [HttpPost("messages")]
        public async Task Post(ChatMessage message)
        {
            // run some logic...
            // System.Console.WriteLine(message.Message);
            await _chatHub.Clients.All.ReceiveMessage(message);
        }

        
    }

    
}
