using System.Diagnostics;
using System.Runtime.CompilerServices;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
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



            //load key vault
            var azureServicetokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServicetokenProvider.KeyVaultTokenCallback));

            var keyuri = $"https://{config["KeyVaultName"]}.vault.azure.net/";

            //config.AddAzureKeyVault(keyuri, keyVaultClient, new DefaultKeyVaultSecretManager());


            //Sending steps to live fitbit instance don't want to corrupt the real data so, sending as 1 step..

            //the FitBitAPI must be pre-populated with an access_token that we have obtained earlier, as long as we run our app within the expiration window and obtain a new token via refresh, we should
            //only have to do this once??


            var api = new FitBitAPI(null);
            
            api.CreateDailySteps(new DateTime(2022, 5, 13), 1);



            return; 
            
            //Log Yesterdays Steps
            var sr = new StepReader();

            //Scrape Steps from Garmin
            var result = await sr.GetYesterdaysSteps(config);

            
            Debug.WriteLine($"Steps = {result}");

            
            Debug.WriteLine($"Sending to FitBit...");

            //Send steps to the Fitbit API
        }


    }
}
