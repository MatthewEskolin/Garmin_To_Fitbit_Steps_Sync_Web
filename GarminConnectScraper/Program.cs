using System.Collections;
using System.Runtime.CompilerServices;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Garmin_To_FitBit_Steps_Sync_Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;

namespace GarminConnectScraper
{
    internal class Program
    {

        static async Task Main(string[] args)
        {

            //Setup Config
            //Will utilize Azure Key Vault.
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>();


            //Load KeyVault
            var credential = new DefaultAzureCredential();
            config.AddAzureKeyVault(new System.Uri("https://kvfitbitapi.vault.azure.net/"), credential);
            var cnfg = config.Build();


#if TEST

            //Update Secrets

            var apiTest = await FitBitAPI.InitializeApi(cnfg);


            //new access_token

            //string at = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIyMkJOVE0iLCJzdWIiOiI4TTNaOUgiLCJpc3MiOiJGaXRiaXQiLCJ0eXAiOiJhY2Nlc3NfdG9rZW4iLCJzY29wZXMiOiJ3YWN0IiwiZXhwIjoxNjUzMTc2MjQzLCJpYXQiOjE2NTMxNDc0NDN9.R9yswVZcCkHyHMuJeN1T0MAVVvvp_WDcU_kJ8Qbbp0Y";

            ////new refresh token
            //string rt = "e05447a33ccb2b56686650487ae49997f57998372d776f2934b3eff242c38256";


            //var r1 = await apiTest.UpdateKeyVault("Fitbit--AccessToken", at);

            //Console.WriteLine();
            //Console.WriteLine(r1.Value.Value);
            //Console.WriteLine("Updated Access Token");

            //var r2 = await apiTest.UpdateKeyVault("Fitbit--RefreshToken", rt);

            //Console.WriteLine();
            //Console.WriteLine(r2.Value.Value);
            //Console.WriteLine("Updated Refresh Token");

            //Update Azure Key Vault Refresh Token
            //"Fitbit--RefreshToken"
            return;
#else




            ////Run Main Program Tasks
            var program = new GarminConnectScraper(cnfg);
            await program.Run();

#endif


        }


    }
}
