using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarminConnectScraper.CookExports
{
    internal class Misc
    {
        //How to initialize a host
        //Initialize Host 
        //using IHost host = Host.CreateDefaultBuilder(args).Build();
        //await host.RunAsync();

        //Old Method of Initializing a connection to Azure KeyVault
        ////load key vault
        //var azureServicetokenProvider = new AzureServiceTokenProvider();
        //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServicetokenProvider.KeyVaultTokenCallback));

        //config.AddAzureKeyVault(keyuri, keyVaultClient, new DefaultKeyVaultSecretManager());

        //var keyuri = $"https://{config["KeyVaultName"]}.vault.azure.net/";




        //HOW TO READ ALL ENVIRONMENT VARIABLES
        //Console.WriteLine();
        //Console.WriteLine("GetEnvironmentVariables: ");
        //foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
        //Console.WriteLine("  {0} = {1}", de.Key, de.Value);




        //HOW TO MANIPUATE AZURE KEYVAULT
        //var keyVaultUri = new System.Uri("https://kvfitbitapi.vault.azure.net/");

        //var client = new SecretClient(keyVaultUri, credential);
        //client.SetSecret(new KeyVaultSecret("Test-Secret", "Test-Value-Updated-2"));
        //client.SetSecret(new KeyVaultSecret("Test-Secret2", "Test-Value2-Updated-2"));

        //Response<KeyVaultSecret>? s1 = await client.GetSecretAsync("Test-Secret");
        //Console.WriteLine(s1.Value.Value);

        //var s2 = await client.GetSecretAsync("Test-Secret2");
        //Console.WriteLine(s2.Value.Value);

        //Console.WriteLine(cnfg["Test-Secret"]);
        //Console.WriteLine(cnfg["Test-Secret2"]);

        //cnfg.Reload();



    }
}
