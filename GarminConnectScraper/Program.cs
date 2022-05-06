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
            var sr = new StepReader();

            await sr.GetYesterdaysSteps(config);

            //Send steps to the Fitbit API
        }


    }
}
