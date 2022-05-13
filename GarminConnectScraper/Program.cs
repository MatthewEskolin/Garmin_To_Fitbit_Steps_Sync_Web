using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;

namespace GarminConnectScraper 
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
            var sr = new StepReader();

            var result = await sr.GetYesterdaysSteps(config);
            
            Debug.WriteLine($"Steps = {result}");

            //Send steps to the Fitbit API
        }


    }
}
