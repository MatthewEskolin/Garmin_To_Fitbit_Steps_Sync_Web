using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;

namespace GarminConnectScraper // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Initialize Host 
            //using IHost host = Host.CreateDefaultBuilder(args).Build();
            //await host.RunAsync();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();


            //Log Yesterdays Steps
            await GetYesterdaysSteps(config);
        }

        private static async Task GetYesterdaysSteps(IConfigurationRoot p_config)
        {
            var config = p_config;

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions() {Headless = false});

            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://connect.garmin.com/signin/", WaitUntilNavigation.Networkidle0);

            //gauth - widget - frame - gauth - widget'

            var elementHandle = page.QuerySelectorAsync("#gauth-widget-frame-gauth-widget");

            var frame = await elementHandle.Result.ContentFrameAsync();


            var userName = config.GetValue<string>("UserName");
            var passWord = config.GetValue<string>("Password");

            Debug.WriteLine($"username={userName};password={passWord}");

            await frame.TypeAsync("#username", userName);
            await frame.TypeAsync("#password", passWord);

           // await frame.WaitForNavigationAsync(new NavigationOptions() {WaitUntil = new []{ WaitUntilNavigation.Networkidle0}});
            await frame.ClickAsync("#login-btn-signin");

            await page.WaitForNavigationAsync();

            //Go to steps daily summary page
            var date = DateTime.Now.ToString("yyyy-mm-dd");

            var stepsTodayUrl = $"https://connect.garmin.com/modern/daily-summary/{date}/steps";

            await page.GoToAsync(stepsTodayUrl, WaitUntilNavigation.Networkidle0);

            var stepsDiv = await page.QuerySelectorAsync( @"#column-0 > div:nth-child(1) > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div");

            var steps = await page.EvaluateFunctionAsync<string>("e => e.textContent", stepsDiv);

            if(!string.IsNullOrEmpty(steps))
            {
                WriteStepsToFile(steps);
            }


            //var frame = await elementHandle.

            //Resources for Next Time

            //https://stackoverflow.com/questions/65049531/puppeteer-iframe-contentframe-returns-null
            //C:\src\gh\garmin-connect-scraper\lib\main.js
            //https://www.puppeteersharp.com/examples/index.html




        }

        private static void WriteStepsToFile(string steps)
        {
            var filePath = @"C:\Output\GarminSteps\MySteps.txt";

            using StreamWriter sw = File.AppendText(filePath);

            sw.WriteLine(steps);
        }
    }
}
