using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PAC.Scraper
{
    class ProductScraper
    {
        private string _url = "";
        private string _selector = "";
        private string _compareValue = "";
        private HtmlWeb _web = null;
        private HtmlDocument _htmlDoc = null;
        private HtmlNode _doc = null;
        private HtmlNode _node = null;
        public int itemAvailable = -1;

        public ProductScraper(string url)
        {
            _url = url;
            _web = new HtmlWeb();
        }

        public async Task<int> CheckForContentMatch(string selector, string compareValue)
        {
            
            _selector = selector;
            _compareValue = compareValue;

            await GetPage().ConfigureAwait(false);
            await GetElement().ConfigureAwait(false);

            if(_node != null)
            {
                if(_node.InnerHtml == _compareValue)
                {
                    return 1;
                } else
                {
                    return 0;
                }
            }
            return 0;
        }

        private async Task GetPage()
        {
            _htmlDoc = await _web.LoadFromWebAsync(_url).ConfigureAwait(false);
            _doc = _htmlDoc.DocumentNode;
        }

        private async Task GetElement()
        {
            _node = _doc.QuerySelector(_selector);
        }
        
    }
}
