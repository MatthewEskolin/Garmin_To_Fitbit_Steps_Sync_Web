using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;

namespace Garmin_To_Fitbit_Steps_Sync_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        //TODO how to have different keys between development-production. Does this key vault always get used.
        //Test to see if local user-secrets override the key-vault - we could create a unit test for this!
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((context, config) =>
                    {
                        var builtConfig = config.Build();

                        var azureServicetokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServicetokenProvider.KeyVaultTokenCallback));

                        var keyuri = $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/";

                        config.AddAzureKeyVault(keyuri, keyVaultClient, new DefaultKeyVaultSecretManager());

                    }).UseStartup<Startup>();
    }

}
