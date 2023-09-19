using System.Globalization;
using System.Text;
using AngleSharp;

namespace priceParser;

class priceParser
{
    public static async Task Main()
    {
        // Define the URL of the website
        const string url = "https://touch.com.ua/item/apple-macbook-air-13-retina-m2-8-core-cpu-10-core-gpu-16-core-neural-engine-8gb-ram-512gb-ssd-starli_1/";
        const bool isDebug = false;

        Console.OutputEncoding = Encoding.UTF8;

        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        int? oldPrice = null;


        while (true)
        {
            try
            {
                // Send an HTTP GET request to the website
                var document = await context.OpenAsync(url);

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