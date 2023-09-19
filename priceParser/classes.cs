using AngleSharp;
using AngleSharp.Dom;
using System.Globalization;
using System.Text;

namespace PriceMonitorApp
{
    class PriceMonitor
    {
        private readonly string url;
        private readonly bool isDebug;
        private readonly WebPageScraper scraper;
        private int? oldPrice;

        public PriceMonitor(string url, bool isDebug)
        {
            this.url = url;
            this.isDebug = isDebug;
            scraper = new WebPageScraper();
        }

        public async Task StartMonitoring()
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                try
                {
                    // Send an HTTP GET request to the website
                    var document = await scraper.GetWebPageAsync(url);

                    if (isDebug)
                    {
                        Console.WriteLine("HTTP request is successful");
                    }

                    // Find the element that contains the price
                    var priceElement = document.QuerySelector("a.price.changePrice");

                    if (priceElement != null)
                    {
                        // Extract the current price (assuming it's in the format "63 139 ₴")
                        var priceText = priceElement.TextContent.Trim().Replace("₴", "").Replace(" ", "");
                        var currentPrice = int.Parse(priceText, CultureInfo.InvariantCulture);
                        var timestamp = DateTime.Now;

                        if (isDebug)
                        {
                            Console.WriteLine("The price is extracted");
                        }

                        // Check if the price has changed
                        if (oldPrice == null)
                        {
                            // First time checking, set the old price and timestamp
                            oldPrice = currentPrice;
                            Console.WriteLine($"Initial price: {currentPrice} ₴ at {timestamp}");
                        }
                        else if (currentPrice != oldPrice)
                        {
                            // Price has changed, send a notification with timestamp
                            Console.WriteLine($"Price changed from {oldPrice} ₴ to {currentPrice} ₴ at {timestamp}.");
                            oldPrice = currentPrice;
                        }
                        else
                        {
                            Console.WriteLine($"No price change detected at {timestamp}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Price element not found on the page.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Failed to fetch the webpage: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle other exceptions as needed
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                // Sleep for 30 minutes (30 * 60 * 1000 milliseconds)
                Thread.Sleep(30 * 60 * 1000);
            }
        }
    }

    class WebPageScraper
    {
        public async Task<IDocument> GetWebPageAsync(string url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(url);
        }
    }

    class Program
    {
        public static async Task Main()
        {
            // Define the URL of the website
            const string url = "https://touch.com.ua/item/apple-macbook-air-13-retina-m2-8-core-cpu-10-core-gpu-16-core-neural-engine-8gb-ram-512gb-ssd-starli_1/";
            const bool isDebug = false;

            var priceMonitor = new PriceMonitor(url, isDebug);
            await priceMonitor.StartMonitoring();
        }
    }
}
