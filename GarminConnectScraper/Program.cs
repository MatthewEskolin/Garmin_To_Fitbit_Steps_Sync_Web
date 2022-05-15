using System.Collections;
using System.Runtime.CompilerServices;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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





            //Update Secrets


            //Update Azure Key Vault Refresh Token
            //"Fitbit--RefreshToken"
            var keyVaultUri = new System.Uri("https://kvfitbitapi.vault.azure.net/");

            var client = new SecretClient(keyVaultUri, credential);
            client.SetSecret(new KeyVaultSecret("Test-Secret", "Test-Value-Updated-2"));
            client.SetSecret(new KeyVaultSecret("Test-Secret2", "Test-Value2-Updated-2"));

            Response<KeyVaultSecret>? s1 = await client.GetSecretAsync("Test-Secret");
            Console.WriteLine(s1.Value.Value);

            var s2 = await client.GetSecretAsync("Test-Secret2");
            Console.WriteLine(s2.Value.Value);

            Console.WriteLine(cnfg["Test-Secret"]);
            Console.WriteLine(cnfg["Test-Secret2"]);

            cnfg.Reload();





            ////Run Main Program Tasks
            //var program = new GarminConnectScraper(cnfg);
            //await program.Run();




        }


    }
}
