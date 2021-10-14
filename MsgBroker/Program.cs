using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using MsgBroker.Controllers;
using MsgBroker.Hubs;
using MsgBroker.Models.AuthorizationScope;
using MsgBroker.Models.Common;
using MsgBroker.Services;

namespace MsgBroker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // create, configure  and inject singleton services
            TSProxyService.Instance.Clients = ((IHubContext<DataHub>)host.Services.GetService(typeof(IHubContext<DataHub>)))?
                .Clients;
            MemoryRepository.Instance.Init("./wwwroot/users.xml");
            DataHub.TsProxy = TSProxyService.Instance;
            //HomeController.MinutesToExpire = 60;
            HomeController.Repository = MemoryRepository.Instance;
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.Limits.MaxConcurrentConnections = 10;
                            serverOptions.Limits.MaxConcurrentUpgradedConnections = 10;
                            serverOptions.Limits.MaxRequestBodySize = 30 * 1024;
                            serverOptions.Limits.MinRequestBodyDataRate =
                                new MinDataRate(bytesPerSecond: 100,
                                    gracePeriod: TimeSpan.FromSeconds(10));
                            serverOptions.Limits.MinResponseDataRate =
                                new MinDataRate(bytesPerSecond: 100,
                                    gracePeriod: TimeSpan.FromSeconds(10));
                            serverOptions.Listen(IPAddress.Loopback, 5432);
                            serverOptions.Limits.KeepAliveTimeout =
                                TimeSpan.FromMinutes(2);
                            serverOptions.Limits.RequestHeadersTimeout =
                                TimeSpan.FromMinutes(1);
                        })
                        .UseStartup<Startup>();
                });
    }
}
