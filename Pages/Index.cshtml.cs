using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages
{

    public class IndexModel : PageModel
    {

        [BindProperty(SupportsGet=true)]
        public string Code{get; set;}
        private readonly ILogger<IndexModel> _logger;

        IConfiguration Configuration {get; set;}
        public string AuthorizationUrl { get; private set; }
        public string htmlResult { get; private set; } = string.Empty;

        public IndexModel(ILogger<IndexModel> logger,IConfiguration config)
        {
            _logger = logger;
            Configuration = config;
        }

        public IActionResult OnGet()
        {
            if(!String.IsNullOrEmpty(Code)){

                    var jsonString = FitbitAuthenticateGetJson();
                    dynamic obj = JsonSerializer.Deserialize<dynamic>(jsonString);


                // return FitbitAuthenticate();
                //get token
            }


            //Setup Authorization URL
            var clientID = Configuration["Fitbit:ClientID"];
            this.AuthorizationUrl = $"https://www.fitbit.com/oauth2/authorize?client_id={clientID}&response_type=code&scope=activity";



            return Page();
        }


        public IActionResult FitbitAuthenticate()
        {
            var contentResult = new ContentResult();
            contentResult.Content = GetAccessToken();
            contentResult.ContentType = "text/html";
            return contentResult;
        }

        public string FitbitAuthenticateGetJson(){

            return GetAccessToken();
        }

        private string GetAccessToken()
        {
                    var tokenUrl = "https://api.fitbit.com/oauth2/token"; 
                    var clientID = Configuration["Fitbit:ClientID"];
                    var clientSecret = Configuration["Fitbit:ClientSecret"];


                    using (var client = new HttpClient())
                    {

                        var postData = new List<KeyValuePair<string, string>>(){
                            new KeyValuePair<string, string>("code", Code),
                            new KeyValuePair<string, string>("grant_type", "authorization_code"),
                            new KeyValuePair<string, string>("client_id", clientID),
                            new KeyValuePair<string, string>("client_secret", clientSecret),
                        };

                        var request = new HttpRequestMessage();
                        request.Method = HttpMethod.Post;
                        request.RequestUri = new Uri(tokenUrl);

                        HttpContent content = new FormUrlEncodedContent(postData);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                        request.Content = content;

                        var authenticationString = $"{clientID}:{clientSecret}";
                        var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
                        request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

                        var responseResult = client.SendAsync(request).GetAwaiter().GetResult();
                        // tokenUrl, content).Result;

                        var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();


                        this.htmlResult = result;

return result;

                        //get daily activities.
                        // var getActivitiesUrl = "https://api.fitbit.com/1/user/[user-id]/activities/date/2020-07-01.json";
                        // var responseResult1 = client.GetAsync(getActivitiesUrl).Result;
                        // var result1 = responseResult1.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    }

          
        }

        public void PrepareTokenRequest(){

            // services.AddAuthentication()
            // .AddFacebook(options =>
            // {
            //     options.AppId = Configuration["Authentication:Facebook:AppId"];
            //     options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            // })
            // .AddGoogle(options =>
            // {
            //     options.ClientId = Configuration["Authentication:Google:ClientId"];
            //     options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            // });




            // var authenticationString = $"{clientId}:{clientSecret}";
            // var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            // content.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

        }


    }
}
