using AngleSharp;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Parser
{
    static async Task Main()
    {
        // Define the URL of the website
        var url =
            "https://touch.com.ua/item/apple-macbook-air-13-retina-m2-8-core-cpu-10-core-gpu-16-core-neural-engine-8gb-ram-512gb-ssd-starli_1/";

        // Define the initial price and timestamp
        int? oldPrice = null;

        bool isDebug = false;
        bool isPriceShown = true;

        // Create HttpClient instance outside the loop
        using (var client = new HttpClient())
        {
            while (true)
            {
                try
                {
                    // Send an HTTP GET request to the website
                    var response = await client.GetAsync(url);
                    if (isDebug)
                    {
                        Console.WriteLine("Sent HTTP request");
                    }

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        if (isDebug)
                        {
                            Console.WriteLine("HTTP request is successful");
                        }

                        // Parse the HTML content of the page
                        var htmlContent = await response.Content.ReadAsStringAsync();

                        // Use AngleSharp for faster HTML parsing
                        var context = BrowsingContext.New();
                        var doc = await context.OpenAsync(req => req.Content(htmlContent));

                        if (isDebug)
                        {
                            Console.WriteLine("HTML page is parsed");
                        }

                        // Find the element that contains the price
                        var priceElement = doc.QuerySelector("a.price.changePrice");
                        if (isDebug)
                        {
                            Console.WriteLine("Found the price element");
                        }

                        if (priceElement != null)
                        {
                            // Extract the current price (assuming it's in the format "63 139 ₴")
                            var priceText = priceElement.TextContent.Trim().Replace("₴", "").Replace(" ", "");
                            var currentPrice = int.Parse(priceText, CultureInfo.InvariantCulture);
                            if (isDebug)
                            {
                                Console.WriteLine("The price is extracted");
                            }

                            // Check if the price has changed
                            if (oldPrice == null)
                            {
                                // First time checking, set the old price and timestamp
                                oldPrice = currentPrice;
                                var timestamp = DateTime.Now;
                                Console.WriteLine($"Initial price: {currentPrice} ₴ at {timestamp}" );
                            }
                            else if (currentPrice != oldPrice)
                            {
                                // Price has changed, send a notification with timestamp
                                var newTimestamp = DateTime.Now;
                                Console.WriteLine(
                                    $"Price changed from {oldPrice} ₴ to {currentPrice} ₴ at {newTimestamp}.");
                                oldPrice = currentPrice;
                            }
                            else
                            {
                                Console.WriteLine($"No price change detected at {DateTime.Now}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Price element not found on the page.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch the webpage. Check your internet connection or the URL.");
                    }
                    Thread.Sleep(5*60*1000); // Sleep for 30 minutes (30 * 60 * 1000 milliseconds)
                }
                catch (Exception ex)
                {
                    // Handle exceptions as needed
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}