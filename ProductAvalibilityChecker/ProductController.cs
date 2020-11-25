using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using PAC.Scraper;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace PAC.Controllers
{
    public class ProductController : WebApiController
    {
        // You need to add a default constructor where the first argument
        // is an IHttpContext
        public ProductController()
            : base()
        {
        }

        // You need to include the WebApiHandler attribute to each method
        // where you want to export an endpoint. The method should return
        // bool or Task<bool>.
        [Route(HttpVerbs.Post, "/check-item")]
        public async Task<int> CheckItem([FormField] string url, [FormField] string selector, [FormField] string compareValue)
        {
            var scraper = new ProductScraper(url);

            return await scraper.CheckForContentMatch(selector, compareValue).ConfigureAwait(false) == 1 ? 0 : 1;
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
}
