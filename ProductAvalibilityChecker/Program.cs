using System;
using IronWebScraper;
using GenHTTP.Engine;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Websites;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Core;
using GenHTTP.Themes.AdminLTE;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Controllers;
using System.Collections.Generic;
using GenHTTP.Modules.Basics;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.IO;

namespace ProductAvalibilityChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread1 = new Thread(delegate ()
            {
                StartServer();
            });
            thread1.Start();

            var browser = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo("http://localhost:8080") { UseShellExecute = true }
            };
            browser.Start();

            Console.ReadKey();

            browser.Close();

            thread1.Interrupt();

        }

        static void StartServer()
        {


            var app = Layout.Create()
                    .AddController<ProductController>("product")
                    .Index(ModRazor.Page(Data.FromFile("xbox-series-x.html"), (r, h) => new ViewModel(r, h))
                    .Title("Xbox Series X"));
                    //.AddService<ProductResource>("api");
                

            var theme = Theme.Create()
                             .Title("Product Availablity Checker");

            var website = Website.Create()
                                 .Theme(theme)
                                 .Content(app);

            Host.Create()
                .Handler(website)
                .Defaults()
                .Development()
                .Console()
                .Run();
        }
    }

    // Model
    //public record Book(int ID, string Title);

    // Controller
    public class ProductController
    {

        public IHandlerBuilder Index()
        {
            return ModRazor.Page(Data.FromFile("xbox-series-x.html"), (r, h) => new ViewModel(r, h))
                             .Title("Xbox Series X");
        }
        [ResourceMethod(RequestMethod.POST)]
        public IHandlerBuilder CheckItem(Stream stream)
        {
            ProductScraper scrape = new ProductScraper("https://www.xbox.com/en-us/configure/8wj714n3rbtl", ".src-pages-BundleBuilder-components-BundleBuilderHeader-__BundleBuilderHeader-module___checkoutButton", "Out of stock");
            // Start Scraping
            scrape.Start();

            return null;
        }

        //[ResourceMethod(RequestMethod.POST)]
        //public int CheckItem(string url, string selector, string compareValue)
        //{
        //    ProductScraper scrape = new ProductScraper("https://www.xbox.com/en-us/configure/8wj714n3rbtl", ".src-pages-BundleBuilder-components-BundleBuilderHeader-__BundleBuilderHeader-module___checkoutButton", "Out of stock");
        //    // Start Scraping
        //    scrape.Start();

        //    return scrape.itemAvailable;
        //}

        //public IHandlerBuilder Create()
        //{
        //    return ModRazor.Page(Data.FromFile("BookCreation.html"))
        //                     .Title("Add Book");
        //}

        //[ControllerAction(RequestMethod.POST)]
        //public IHandlerBuilder Create(string title)
        //{
        //    //var book = new Book(_Books.Max(b => b.ID) + 1, title);

        //    //_Books.Add(book);

        //    return Redirect.To("{index}/", true);
        //}

        //public IHandlerBuilder Edit([FromPath] int id)
        //{
        //    //var book = _Books.Where(b => b.ID == id).First();

        //    return ModRazor.Page(Data.FromFile("BookEditor.html"), (r, h) => new ViewModel(r, h, book))
        //                     .Title(book.Title);
        //}



        //[ControllerAction(RequestMethod.POST)]
        //public IHandlerBuilder CheckItem()
        //{


        //    //return Redirect.To("{index}/", true);
        //}        //[ControllerAction(RequestMethod.POST)]
        //public IHandlerBuilder CheckItem()
        //{


        //    //return Redirect.To("{index}/", true);
        //}

        //[ControllerAction(RequestMethod.POST)]
        //public IHandlerBuilder Delete([FromPath] int id)
        //{
        //    _Books.RemoveAll(b => b.ID == id);

        //    return Redirect.To("{index}/", true);
        //}

    }

    

    //public class ProductResource
    //{
    //    [ResourceMethod(RequestMethod.POST)]
    //    public int CheckItem(string url, string selector, string compareValue)
    //    {
    //        ProductScraper scrape = new ProductScraper("https://www.xbox.com/en-us/configure/8wj714n3rbtl", ".src-pages-BundleBuilder-components-BundleBuilderHeader-__BundleBuilderHeader-module___checkoutButton", "Out of stock");
    //        // Start Scraping
    //        scrape.Start();

    //        return scrape.itemAvailable;
    //    }
    //}

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
