﻿using System.Runtime.CompilerServices;
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


            //Run Main Program Tasks
            var program = new GarminConnectScraper(cnfg);
            await program.Run();




        }


    }
}
