using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages
{
    public partial class IndexModel : PageModel
    {
        //Public Properites
        #region Properties
        [FromForm]
        [BindProperty]
        public AuthorizationResponse Authorization { get; set; }

        [BindProperty]
        public int Steps {get; set;}

        ///Authoriziation Code from Auth flow
        [BindProperty(SupportsGet=true)] 
        public string Code { get; set; }

        //System messages and debugging
        public string SystemMessage { get; set; }

        //display connection state to the user
        [BindProperty]
        [Display(Name = "Connection State")]
        public string ConnectionState { get; set; }

        public string AuthorizationUrl { get; set; }

        //1 = connected; 0 = Other State
        [BindProperty]
        public int ConnectionStateCode { get; set; }

        // public List<DateTime> AvailableDates {get; set;}
        public List<SelectListItem> AvailableDatesSelect { get;  set; }

        [Display(Name = "Activity Date")]
        [BindProperty]
        public DateTime ActivityDate {get; set;}


           public List<int> Ints {get; set;} = new List<int>(){2,3,4,5} ;

        private readonly ILogger<IndexModel> _logger;
        private IConfiguration Configuration { get; set; }

        #endregion

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            Configuration = config;

            //set up Authorizstion URL
            var clientID = Configuration["Fitbit:ClientID"];
            this.AuthorizationUrl = $"https://www.fitbit.com/oauth2/authorize?client_id={clientID}&response_type=code&scope=activity";

            //setup available dates
            //TODAY - YESTERDAY - DAYBEFOREYESTERDAY
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var daybeforeyesterady = today.AddDays(-2);


            // AvailableDates = new List<DateTime>(){today,yesterday, daybeforeyesterady};
            AvailableDatesSelect = new List<SelectListItem>(){


                new SelectListItem(){Text = $"Today - {today.ToShortDateString()}", Value = today.ToShortDateString()},
                new SelectListItem(){Text = $"Yesterday - {yesterday.ToShortDateString()}", Value = yesterday.ToShortDateString()},
                new SelectListItem(){Text = $"Anteayer - {daybeforeyesterady.ToShortDateString()}", Value = daybeforeyesterady.ToShortDateString()}


            };

        }
        public void OnGet()
        {
            ConnectionState = "Ready To Connect";
            Steps = 1;
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
        public void OnPostCreateActivity()
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


                if(responseResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //attempt to deserialize error state
                    try{

                        var serializedresult = JsonSerializer.Deserialize<ErrorRoot>(result);
                        SystemMessage = serializedresult.errors[0].message;
                        return;

                    }
                    catch
                    {
                        SystemMessage = "Activty Creation Failed - Unknown Response Type.";
                        return;
                    }
                }
                else
                {
                    //Should be an activity root returned
                    var serializedresult = JsonSerializer.Deserialize<ActivityTypeRoot>(result);
                    string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });

                }

                //*Json Can be Deserialied and then Serialized again in order to format with with indents for readabiility
                // var serializedresult = JsonSerializer.Deserialize<Root>(result);
                // string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });
                // var steps = serializedresult.summary.steps;

                //*Uncomment below lineto get result returned from API
                 //return new ContentResult { Content = result, ContentType = "application/json" };

                 SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";

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
                

                var serializedresult = JsonSerializer.Deserialize<ActivityTypeRoot>(result);

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
                this.Authorization = obj;
                this.ConnectionState = "Connected";
                this.ConnectionStateCode = 1;
                this.Steps = 1;
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




namespace folding_example_LEVEL_1
{
    public class folding_example_LEVEL_2
    {
        public class folding_example_LEVEL_3{

        }

        public void folding_Example_LEVEL_3(){

            if(true)
            { // LEVEL 4

                
            }

        }

    }




}













