using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

namespace GarminConnectScraper
{
    public class StepReader
    {
        private Page CurrentPage { get; set; }

        public async Task<int> GetYesterdaysSteps(IConfigurationRoot p_config)
        {
            var config = p_config;

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            await using Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = false, Args = new[] { "--window-size=1920,1080" } });

            CurrentPage  = await browser.NewPageAsync();


            await CurrentPage.GoToAsync("https://connect.garmin.com/signin/", WaitUntilNavigation.Networkidle0);

            //gauth - widget - frame - gauth - widget'

            var elementHandle = CurrentPage.QuerySelectorAsync("#gauth-widget-frame-gauth-widget");

            var frame = await elementHandle.Result.ContentFrameAsync();


            var userName = config.GetValue<string>("UserName");
            var passWord = config.GetValue<string>("Password");

            Debug.WriteLine($"username={userName};password={passWord}");

            await frame.TypeAsync("#username", userName);
            await frame.TypeAsync("#password", passWord);

            // await frame.WaitForNavigationAsync(new NavigationOptions() {WaitUntil = new []{ WaitUntilNavigation.Networkidle0}});
            await frame.ClickAsync("#login-btn-signin");

            await CurrentPage.WaitForNavigationAsync();

            //Go to steps daily summary page
            //var date = DateTime.Now.ToString("yyyy-MM-dd");

            //var stepsTodayUrl = $"https://connect.garmin.com/modern/daily-summary/{date}/steps";

            //await page.GoToAsync(stepsTodayUrl, WaitUntilNavigation.Networkidle0);

            //var stepsDiv = await page.QuerySelectorAsync( @"#column-0 > div:nth-child(1) > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div");
            var stepsDiv = await GetStepsDiv();

            if (stepsDiv == null)
            {
                Console.WriteLine("Could not Find Element");
                return 0;
            }
            else
            {
                Console.WriteLine("Found Steps Element.. continuing...");
            }

            var steps = await CurrentPage.EvaluateFunctionAsync<string>("e => e.textContent", stepsDiv);

            if (!string.IsNullOrEmpty(steps))
            {
                steps = steps.Replace(",", "");
                WriteStepsToFile(steps);

                return Int32.Parse(steps);
            }

            //var frame = await elementHandle.

            //Resources for Next Time

            //https://stackoverflow.com/questions/65049531/puppeteer-iframe-contentframe-returns-null
            //C:\src\gh\garmin-connect-scraper\lib\main.js
            //https://www.puppeteersharp.com/examples/index.html




        }

        private async Task<ElementHandle?> GetStepsDiv()
        {
            //Try to find the steps element on the page.
            ElementHandle? rtn = null;

            var path1 = "#column-0 > div.widget.widget-large.large.steps > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div";
            var path2 = "#column-0 > div:nth-child(9) > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div";
                        //"#column-0 > div.widget.widget-large.large.steps > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div")

            rtn = await CurrentPage.QuerySelectorAsync(path1);
            if (rtn != null) return rtn;

            rtn = await CurrentPage.QuerySelectorAsync(path2);
            if (rtn != null) return rtn;

            return null;
        }

        private  void WriteStepsToFile(string steps)
        {
            var filePath = @"C:\Output\GarminSteps\MySteps.txt";

            using StreamWriter sw = File.AppendText(filePath);

            sw.WriteLine(steps);
        }

    }
}
