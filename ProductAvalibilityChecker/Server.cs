using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using PAC.Controllers;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PAC.Servers
{
    class MainServer
    {
        // Create and configure our web server.
        public static WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m
                    .WithController<ProductController>())
                .WithModule(new WebSocketsChatServer("/chat"))
                .WithStaticFolder("/", Environment.CurrentDirectory, true, m => m
                    .WithContentCaching(true)) // Add static files after other modules to avoid conflicts
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        static void StartServer()
        {

        }
    }

    
}
