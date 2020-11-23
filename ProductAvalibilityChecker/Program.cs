using System;
using IronWebScraper;
using System.Threading;
using System.IO;
using EmbedIO.WebSockets;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using EmbedIO.Actions;
using EmbedIO.Files;
using Swan.Logging;
using EmbedIO.Routing;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ProductAvalibilityChecker
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var url = "http://localhost:8080/";
            if (args.Length > 0)
                url = args[0];

            // Our web server is disposable.
            using (var server = CreateWebServer(url))
            {
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                server.RunAsync();

                var browser = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }
                };
                browser.Start();
                // Wait for any key to be pressed before disposing of our web server.
                // In a service, we'd manage the lifecycle of our web server using
                // something like a BackgroundWorker or a ManualResetEvent.
                Console.ReadKey(true);
            }


        }

        // Create and configure our web server.
        private static WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m
                    .WithController<PeopleController>())
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

    public class PeopleController : WebApiController
    {
        // You need to add a default constructor where the first argument
        // is an IHttpContext
        public PeopleController()
            : base()
        {
        }

        // You need to include the WebApiHandler attribute to each method
        // where you want to export an endpoint. The method should return
        // bool or Task<bool>.
        [Route(HttpVerbs.Post, "/check-item")]
        public async Task<int> CheckItem([FormField] string url, [FormField] string selector, [FormField] string compareValue)
        {
            var scraper = new ProductScraper(url, selector, compareValue);
            await scraper.StartAsync().ConfigureAwait(false);
            return scraper.itemAvailable;
        }

        [Route(HttpVerbs.Post, "/send-sms")]
        public async Task SendSMS([FormField] string twiliophone, [FormField] string cellphone, [FormField] string tkey, [FormField] string tvalue, [FormField] string url, [FormField] int isavailable)
        {
            TwilioClient.Init(tkey, tvalue);
            string prefstr = isavailable == 1 ? "AVAILABLE: " : "UNAVAILABLE: ";
            var message = await MessageResource.CreateAsync(
                body: $"{prefstr}{url}",
                from: new Twilio.Types.PhoneNumber(twiliophone),
                to: new Twilio.Types.PhoneNumber(cellphone)
            ).ConfigureAwait(false);
        }

    }


    /// <summary>
    /// Defines a very simple chat server.
    /// </summary>
    public class WebSocketsChatServer : WebSocketModule
        {
            public WebSocketsChatServer(string urlPath)
                : base(urlPath, true)
            {
                // placeholder
            }

            /// <inheritdoc />
            protected override Task OnMessageReceivedAsync(
                IWebSocketContext context,
                byte[] rxBuffer,
                IWebSocketReceiveResult rxResult)
                => SendToOthersAsync(context, Encoding.GetString(rxBuffer));

            /// <inheritdoc />
            protected override Task OnClientConnectedAsync(IWebSocketContext context)
                => Task.WhenAll(
                    SendAsync(context, "Welcome to the chat room!"),
                    SendToOthersAsync(context, "Someone joined the chat room."));

            /// <inheritdoc />
            protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
                => SendToOthersAsync(context, "Someone left the chat room.");

            private Task SendToOthersAsync(IWebSocketContext context, string payload)
                => BroadcastAsync(payload, c => c != context);
        }



    class ProductScraper : WebScraper
    {
        private string url = "";
        private string selector = "";
        private string compareValue = "";
        public int itemAvailable = -1;

        public ProductScraper(string url, string selector, string compareValue)
        {
            this.url = url;
            this.selector = selector;
            this.compareValue = compareValue;
        }
        public override void Init()
        {
            this.LoggingLevel = WebScraper.LogLevel.All;

            // Gets or sets the total number of allowed open HTTP requests (threads)
            this.MaxHttpConnectionLimit = 80;
            // Gets or sets minimum polite delay (pause)between request to a given domain or IP address.
            this.RateLimitPerHost = TimeSpan.FromMilliseconds(50);
            //     Gets or sets the allowed number of concurrent HTTP requests (threads) per hostname
            //     or IP address. This helps protect hosts against too many requests.
            this.OpenConnectionLimitPerHost = 25;
            this.ObeyRobotsDotTxt = false;
            //     Makes the WebSraper intelligently throttle requests not only by hostname, but
            //     also by host servers' IP addresses. This is polite in-case multiple scraped domains
            //     are hosted on the same machine.
            this.ThrottleMode = Throttle.ByDomainHostName;
            this.Request(this.url, Parse);
        }

        public override void Parse(Response response)
        {
            if (response.CssExists(this.selector) == true)
            {
                if (compareValue != "" && response.Css(this.selector)[0].InnerHtml == this.compareValue)
                {
                    this.itemAvailable = 1;
                }
                else
                {
                    this.itemAvailable = 0;
                }
            }
        }
    }
}
