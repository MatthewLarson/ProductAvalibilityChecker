using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PAC.Objects;

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
        public Site site = null;

        public ProductScraper(string url)
        {
            _url = url;
            _web = new HtmlWeb();
            _web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36";
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

        private Site getSite(int id)
        {
            switch (id)
            {
                case 1:
                    return new Site { url = "https://www.xbox.com/en-us/configure/8wj714n3rbtl", selector = ".src-pages-BundleBuilder-components-BundleBuilderHeader-__BundleBuilderHeader-module___checkoutButton", compareValue = "Out of stock" };
                case 2:
                    return new Site { url = "https://www.xbox.com/en-us/configure/8wj714n3rbtl", selector = ".src-pages-BundleBuilder-components-BundleBuilderHeader-__BundleBuilderHeader-module___checkoutButton", compareValue = "Out of stock" };
                case 3:
                    return new Site { url = "https://www.xbox.com/en-us/configure/8wj714n3rbtl", selector = ".src-pages-BundleBuilder-components-BundleBuilderHeader-__BundleBuilderHeader-module___checkoutButton", compareValue = "Out of stock" };
                default:
                    return null;
            }
        }
        
    }
}
