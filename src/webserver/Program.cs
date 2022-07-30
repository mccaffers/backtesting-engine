using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Webserver.Api.Hubs;
using Webserver.Api.Hubs.Clients;

namespace Webserver.Api
{
    public class Program
    {
        public static IHubContext<ChatHub, IChatClient>? hubContext;
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            hubContext = host.Services.GetService<IHubContext<ChatHub, IChatClient>>();
            host.Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
