using System;
using PuppeteerSharp;

namespace GarminConnectScraper // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Log Yesterdays Steps
            await GetYesterdaysSteps();
        }

        private static async Task GetYesterdaysSteps()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions() {Headless = true});

            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://connect.garmin.com/signin/", WaitUntilNavigation.Networkidle0);

            //gauth - widget - frame - gauth - widget'

            var elementHandle = page.QuerySelectorAsync("#gauth-widget-frame-gauth-widget");

            var frame = await elementHandle.Result.ContentFrameAsync();

            await frame.TypeAsync("#username", "myusername");
            await frame.TypeAsync("#username", "myusername");





            //var frame = await elementHandle.

            //Resources for Next Time

            //https://stackoverflow.com/questions/65049531/puppeteer-iframe-contentframe-returns-null
            //C:\src\gh\garmin-connect-scraper\lib\main.js
            //https://www.puppeteersharp.com/examples/index.html




        }
    }
}
