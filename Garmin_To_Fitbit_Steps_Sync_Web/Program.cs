using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Garmin_To_Fitbit_Steps_Sync_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }




 public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((context,config) =>
                    {
                        var builtConfig = config.Build();

                        //Old Way
                        //var azureServicetokenProvider = new AzureServiceTokenProvider();
                        //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServicetokenProvider.KeyVaultTokenCallback));

                        //var secretClient = new SecretClient(new Uri(builtConfig["KeyVaultName"]), new DefaultAzureCredential());

                        //var keyuri = $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/";

                        //config.AddAzureKeyVault(keyuri,secretClient,new DefaultKeyVaultSecretManager());

                        var credential = new DefaultAzureCredential();
                        config.AddAzureKeyVault(new System.Uri("https://kvfitbitapi.vault.azure.net/"), credential);


                    }).UseStartup<Startup>();
    }


    





}
