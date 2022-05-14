using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
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



            //await using Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = true, Args = new[] { "--window-size=1920,1080 " +
            //    "                                                                                                            --remote-debugging-port=9222" } });

            await using Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = false, Args = new[]
            {
                "--disable-web-security", "--window-size=1920,1080",
                "--disable-features=IsolateOrigins",
                "--disable-site-isolation-trials"
            } });



            //await using Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = true, Args = new[] { "--window-size=1920,1080" } });

            CurrentPage  = await browser.NewPageAsync();
            CurrentPage.Console += LogToConsole;

            //await CurrentPage.SetRequestInterceptionAsync(true);

            //CurrentPage.Request += RemoveHeader;
            ////CurrentPage.Response += RemoveHeader;
            //CurrentPage.RequestFinished += Finished;


            await CurrentPage.GoToAsync("https://connect.garmin.com/signin/", WaitUntilNavigation.Networkidle0);

            //await CurrentPage.WaitForTimeoutAsync(90000);

            //gauth - widget - frame - gauth - widget'

            var elementHandle = CurrentPage.QuerySelectorAsync("#gauth-widget-frame-gauth-widget");

            var frame = await elementHandle.Result.ContentFrameAsync();
            
            var userName = config.GetValue<string>("UserName");
            var passWord = config.GetValue<string>("Password");

            //Debug.WriteLine($"username={userName};password={passWord}");

            await frame.TypeAsync("#username", userName);
                await frame.TypeAsync("#password", passWord);

            // await frame.WaitForNavigationAsync(new NavigationOptions() {WaitUntil = new []{ WaitUntilNavigation.Networkidle0}});
            await frame.ClickAsync("#login-btn-signin");

            NavigationOptions navOptions = new NavigationOptions
            {
                WaitUntil = new[] {WaitUntilNavigation.Networkidle0}
            };

            

            var startwaiting = CurrentPage.WaitForNavigationAsync(navOptions);

            //await Task.Delay(200);

            //await CurrentPage.ScreenshotAsync($@"C:\Output\GarminSteps\Screenshots\{Guid.NewGuid().ToString()}.jpg");

            await startwaiting;


            //Go to steps daily summary page
            //var date = DateTime.Now.ToString("yyyy-MM-dd");

            //var stepsTodayUrl = $"https://connect.garmin.com/modern/daily-summary/{date}/steps";

            //await page.GoToAsync(stepsTodayUrl, WaitUntilNavigation.Networkidle0);

            //click the back arrow to bring the steps back one day
            await NavigateStepsBackOneDay();


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

            return 0;


            //var frame = await elementHandle.

            //Resources for Next Time

            //https://stackoverflow.com/questions/65049531/puppeteer-iframe-contentframe-returns-null
            //C:\src\gh\garmin-connect-scraper\lib\main.js
            //https://www.puppeteersharp.com/examples/index.html




        }

        private async Task NavigateStepsBackOneDay()
        {
            //Try to find the steps element on the page.
            ElementHandle? handle = null;


            var path1 = "#column-0 > div:nth-child(1) > div.widget-footer.clearfix > div.pull-left > span.navButtons > button.widget-next.widget-footer-action > i";
            var path2 = "#column-0 > div:nth-child(7) > div.widget-footer.clearfix > div.pull-left > span.navButtons > button.widget-next.widget-footer-action > i";
            var path3 = "#column-0 > div:nth-child(8) > div.widget-footer.clearfix > div.pull-left > span.navButtons > button.widget-next.widget-footer-action > i";

            //document.querySelector("#column-0 > div:nth-child(9) > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div")

            var paths = new List<string>() {path1, path2, path3};

            foreach (var path in paths)
            {
                handle = await CurrentPage.QuerySelectorAsync(path);
                if (handle == null) continue;

                await ClickArrow();
                break;
            }

            async Task ClickArrow()
            {
                await handle.ClickAsync();

                //wait for yesterdays steps to appear
                await Task.Delay(2000);


                //CurrentPage.wait

                //NavigationOptions navOptions = new NavigationOptions
                //{
                //    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                //};

                //await CurrentPage.WaitForNavigationAsync(navOptions);
            }
        }

        private async void Finished(object? sender, RequestEventArgs e)
        {
            var res = e.Request.Response;

            if (res.Headers.ContainsKey("X-Frame-Options"))
            {
                res.Headers.Remove("X-Frame-Options");
            }

            //e.Request.Headers
            //var payLoad = CreateInstance<Payload>();

            //payLoad.Headers = e.Request.Headers;

           // await e.Request.RespondAsync(res);

        }

        private void RemoveHeader(object? sender, ResponseCreatedEventArgs e)
        {
            //if (e.Response.Headers.ContainsKey("X-Frame-Options"))
            //{
            //    e.Response.Headers.Remove("X-Frame-Options");
            //}

            ////e.Request.Headers
            ////var payLoad = CreateInstance<Payload>();

            ////payLoad.Headers = e.Request.Headers;

            //await e.Response.();
        }

        public static T CreateInstance<T>(params object[] args)
        {
            // apologies in advance
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }

        private async void RemoveHeader(object? sender, RequestEventArgs e)
        {

            //if (e.Request.Headers.ContainsKey("X-Frame-Options"))
            //{
            //    e.Request.Headers.Remove("X-Frame-Options");
            //}

            //e.Request.Headers
            //var payLoad = CreateInstance<Payload>();

            //payLoad.Headers = e.Request.Headers;

            await e.Request.ContinueAsync();



        }

        private void LogToConsole(object? sender, ConsoleEventArgs e)
        {
            Console.WriteLine(e.Message.Text);
        }

        private async Task<ElementHandle?> GetStepsDiv()
        {
            //Try to find the steps element on the page.
            ElementHandle? rtn = null;

            var path1 = "#column-0 > div.widget.widget-large.large.steps > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div";
            var path2 = "#column-0 > div:nth-child(9) > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div";
            //document.querySelector("#column-0 > div:nth-child(9) > div.widget-content > div.chart-placeholder > div.chart-container > div > div > div > div > span > div > div")


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
