using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Garmin_To_Fitbit_Steps_Sync_Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;

        }
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment CurrentEnvironment { get; set; } 

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            //will need to use an actual distributed memory cache if we want to expand to multiple servers.
            services.AddSession();
            services.AddDistributedMemoryCache();

            if(CurrentEnvironment.IsProduction())
            {
                //Put Production Only Configuration here.
                services.AddApplicationInsightsTelemetry(Configuration);
            }
            else
            {
                // The following line enables Application Insights telemetry collection.
                services.AddApplicationInsightsTelemetry(Configuration);
            }


            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseSession();
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
