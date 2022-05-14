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




    }
}
