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
    [BindProperties(SupportsGet = true)]
    public partial class IndexModel : PageModel
    {




        //NEED TO TURN MY GET ACTIVITIES INTO A MAJOR POST!
        //Public Properites

        [FromForm]
        [BindProperty]
        public AuthorizationResponse Authorization { get; set; }


        public int Steps {get; set;}

        ///Authoriziation Code from Auth flow
        public string Code { get; set; }

        //System messages and debugging
        public string SystemMessage { get; set; }

        //display connection state to the user
        public string ConnectionState { get; set; }

        public string AuthorizationUrl { get; set; }

        //1 = connected; 0 = Other State
        public int ConnectionStateCode { get; set; }


        private readonly ILogger<IndexModel> _logger;
        private IConfiguration Configuration { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            Configuration = config;

            //set up Authorizstion URL
            var clientID = Configuration["Fitbit:ClientID"];
            this.AuthorizationUrl = $"https://www.fitbit.com/oauth2/authorize?client_id={clientID}&response_type=code&scope=activity";
        }

        public void OnGet()
        {
            //how do we know if we are still conected if we have the access token
            //so we may need to store the access token as a session variable?
        }

        public void OnPost()
        {
            //test
        }


        //gets activity types so we can find out what ID to use for walking
        public ContentResult OnPostActivityTypes()
        {
            var url_getActivityTypes = "https://api.fitbit.com/1/activities.json";


            using (var client = new HttpClient())
            {

                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(url_getActivityTypes);

                request.Headers.Add("Authorization", $"Bearer {this.Authorization.access_token}");

                var responseResult = client.SendAsync(request).GetAwaiter().GetResult();

                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // var serializedresult = JsonSerializer.Deserialize<Root>(result);

                // string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions(){WriteIndented = true});

                // var steps = serializedresult.summary.steps;

                return new ContentResult { Content = result, ContentType = "application/json" };


            }
        }



        public ContentResult OnPostCreateActivity()
        {
            //Create's an activity inside my fitbit account for the day with the input number of steps.

            var postCreateNewActivityUrl = "https://api.fitbit.com/1/user/-/activities.json";

            using (var client = new HttpClient())
            {

                var steps = Steps.ToString();

                var today = DateTime.Now.ToString("yyyy-MM-dd");
                var postData = new List<KeyValuePair<string, string>>(){

                            //17190 - walking 3.0 mph pace
                            new KeyValuePair<string, string>("activityId", "17190"),
                            new KeyValuePair<string, string>("startTime", "12:00"),
                            new KeyValuePair<string, string>("durationMillis", "86399999"),
                            new KeyValuePair<string, string>("date", today),
                            new KeyValuePair<string, string>("distanceUnit", "steps"),
                            new KeyValuePair<string, string>("distance", steps),
                        };


                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(postCreateNewActivityUrl);

                HttpContent content = new FormUrlEncodedContent(postData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                request.Content = content;

                request.Headers.Add("Authorization", $"Bearer {this.Authorization.access_token}");

                var responseResult = client.SendAsync(request).GetAwaiter().GetResult();

                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Json Can be Deserialied and then Serialized again in order to format with with indents for readabiility

                // var serializedresult = JsonSerializer.Deserialize<Root>(result);
                // string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });
                // var steps = serializedresult.summary.steps;

                return new ContentResult { Content = result, ContentType = "application/json" };

            }


        }

        public ContentResult OnPostDailyActivities([FromForm] AuthorizationResponse test)
        {

            var getActivitiesUrl = "https://api.fitbit.com/1/user/-/activities/date/2020-07-01.json";

            using (var client = new HttpClient())
            {

                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(getActivitiesUrl);



                // HttpContent content = new ();
                // content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                // request.Content = new StringContent(string.Empty);
                request.Headers.Add("Authorization", $"Bearer {this.Authorization.access_token}");

                var responseResult = client.SendAsync(request).GetAwaiter().GetResult();


                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var serializedresult = JsonSerializer.Deserialize<Root>(result);

                string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });

                //var steps = serializedresult.summary.steps;

                return new ContentResult { Content = jsonFormatted, ContentType = "application/json" };


            }
        }

        public void OnGetAuthorised()
        {

            //received authorization code, so proceed to get access token.
            if (!String.IsNullOrEmpty(Code))
            {

                var jsonString = GetAccessToken();
                AuthorizationResponse obj = JsonSerializer.Deserialize<AuthorizationResponse>(jsonString);
                this.SystemMessage = obj.access_token;
                this.Authorization = obj;
                this.ConnectionState = "Connected";
                this.ConnectionStateCode = 1;
            }
            else
            {
                this.SystemMessage = "Access Code Not Received";
            }

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

                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                return result;

            }
        }
    }
}
