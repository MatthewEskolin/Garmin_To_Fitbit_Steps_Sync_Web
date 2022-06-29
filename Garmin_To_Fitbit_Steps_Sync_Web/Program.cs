using Azure.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Garmin_To_Fitbit_Steps_Sync_Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((context, config) =>
                    {

                        var credential = new DefaultAzureCredential();
                        config.AddAzureKeyVault(new System.Uri("https://kvfitbitapi.vault.azure.net/"), credential);


                    }).UseStartup<Startup>();
    }




}
