using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages
{
    public partial class IndexModel : PageModel
    {
        //Public Properites
        #region Properties

        public AuthorizationResponse Authorization
        {
            get
            {

                byte[] authorizationResponse = null;
                bool inSession = HttpContext.Session.TryGetValue("AuthorizationResponse", out authorizationResponse);
                if (inSession)
                {
                        string json = System.Text.Encoding.UTF8.GetString(authorizationResponse);
                        var authObject = JsonSerializer.Deserialize<AuthorizationResponse>(json);

                        return authObject;

                }
                else
                {
                    return null;
                }

            }
            set
            {
                string json = JsonSerializer.Serialize(value, new JsonSerializerOptions() { WriteIndented = true });
                byte[] serializedResult = System.Text.Encoding.UTF8.GetBytes(json);

                HttpContext.Session.Set("AuthorizationResponse", serializedResult);
            }
        }

        /// <summary>
        /// Number of Steps 
        /// </summary>
        [BindProperty]
        public int Steps { get; set; }

        /// <summary>
        /// Authorization Code which is used to obtain access token
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }

        /// <summary>
        /// Display value to show if we user is connected to the FitBit API.
        /// </summary>
        [BindProperty]
        [Display(Name = "Connection State")]
        public string ConnectionState { get; set; }


        //1 = connected; 0 = Other State
        [BindProperty]
        public int ConnectionStateCode { get; set; }

        public bool IsConnected => ConnectionStateCode == 1;


        [Display(Name = "Activity Date")]
        [BindProperty]
        public DateTime ActivityDate { get; set; }

        //Step Data
        [Display(Name = "Last 7 Days Steps")]
        public long? LastSevenDaysSteps { get; set; }
        [Display(Name = "Last 7 Days Average")]
        public long? LastSevenDaysAverageSteps { get; set; }


        //Properties which are not bound

        private readonly ILogger<IndexModel> _logger;

        private TelemetryClient telemetry;

        private IConfiguration Configuration { get; set; }

        public List<SelectListItem> AvailableDatesSelect { get; set; }
        //System messages and debugging
        public string SystemMessage { get; set; }
        public string AuthorizationUrl { get; set; }
        #endregion

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config,TelemetryClient tel)
        {

            telemetry = tel;
            telemetry.TrackPageView("Index");

            _logger = logger;
            Configuration = config;

            //set up Authorizstion URL
            var clientID = Configuration["Fitbit:ClientID"];
            this.AuthorizationUrl = $"https://www.fitbit.com/oauth2/authorize?client_id={clientID}&response_type=code&scope=activity";

            //setup available dates
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var daybeforeyesterday = today.AddDays(-2);
            var daysMinus3 = today.AddDays(-3);
            var daysMinus4 = today.AddDays(-4);
            var daysMinus5 = today.AddDays(-5);
            var daysMinus6 = today.AddDays(-6);
            var daysMinus7 = today.AddDays(-7);


            // AvailableDates = new List<DateTime>(){today,yesterday, daybeforeyesterady};
            AvailableDatesSelect = new List<SelectListItem>(){


                new SelectListItem(){Text = $"Today - {today.ToShortDateString()}", Value = today.ToShortDateString()},
                new SelectListItem(){Text = $"Yesterday - {yesterday.ToShortDateString()}", Value = yesterday.ToShortDateString()},
                new SelectListItem(){Text = $"{daybeforeyesterday.ToShortDateString()}", Value = daybeforeyesterday.ToShortDateString()},
                new SelectListItem(){Text = $"{daysMinus3.ToShortDateString()}", Value = daysMinus3.ToShortDateString()},
                new SelectListItem(){Text = $"{daysMinus4.ToShortDateString()}", Value = daysMinus4.ToShortDateString()},
                new SelectListItem(){Text = $"{daysMinus5.ToShortDateString()}", Value = daysMinus5.ToShortDateString()},
                new SelectListItem(){Text = $"{daysMinus6.ToShortDateString()}", Value = daysMinus6.ToShortDateString()},
                new SelectListItem(){Text = $"{daysMinus7.ToShortDateString()}", Value = daysMinus7.ToShortDateString()}


            };

        }

        public void OnGet()
        {
            //Initial Page Load -> We don't have an access token yet, unless we had it stored in a database. Could look into persisting between page visits?

            ConnectionState = "Ready To Connect";
            Steps = 1;
        }

        #region Authentication Methods to Connect to FitBit API
        //Authentication Methods to Connect to FitBit API


        //Called from fitbit.com after Authorise is complete. Returned Code is then used to request an access token as part of OAuth flow.
        public void OnGetAuthorised()
        {

            //received authorization code, so proceed to get access token.
            if (!String.IsNullOrEmpty(Code))
            {

                //only get an access token if we don't already have an authoriziation response

                if (this.Authorization == null)
                {
                    var jsonString = GetAccessToken();
                    AuthorizationResponse obj = JsonSerializer.Deserialize<AuthorizationResponse>(jsonString);
                    this.Authorization = obj;
                }


                this.ConnectionState = "Connected";
                this.ConnectionStateCode = 1;
                this.Steps = 1;

                //This section of codes assumes code will be valid and that an access code is returned. Might be good to validate this.
                telemetry.TrackEvent("User Logged In To Fitbit");

                //Load User Data
                GetStepData();

            }
            else
            {
                this.SystemMessage = "Access Code Not Received";
            }

        }
        // This method gets the access token which will be used to access the user's account in the Fitbit API.
        private string GetAccessToken()
        {
            var tokenUrl = "https://api.fitbit.com/oauth2/token";
            var clientID = Configuration["Fitbit:ClientID"];
            var clientSecret = Configuration["Fitbit:ClientSecret"];

            using var client = new HttpClient(handler: new HttpClientHandler
            {
                // 8888 = Fiddler standard port
                Proxy = new WebProxy(new Uri("http://localhost:8888")),
                UseProxy = true
            });



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

        public void OnPostGetRefreshToken()
        {
            var tokenUrl = "https://api.fitbit.com/oauth2/token";

            using (var client = new HttpClient())
            {

                var postData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("refresh_token", Authorization.refresh_token),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                };

                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(tokenUrl);

                HttpContent content = new FormUrlEncodedContent(postData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                request.Content = content;


                var clientID = Configuration["Fitbit:ClientID"];
                var clientSecret = Configuration["Fitbit:ClientSecret"];
                var authenticationString = $"{clientID}:{clientSecret}";
                var base64EncodedAuthenticationString =
                    Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
                request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

                var responseResult = client.SendAsync(request).GetAwaiter().GetResult();

                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var json = JObject.Parse(result);





                //return new ContentResult {Content = result, ContentType = "application/json"};
            }

        }


        #endregion

        //gets activity types so we can find out what ID to use for walking. This returns a lookup table that is not specific to a user
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


        //Creates an Activity  
        public void OnPostCreateActivity()
        {
            //TODO_REFACTOR Does it make sense to bind the dependencies for this method as a parameter.

            //Create's an activity inside my fitbit account for the day with the input number of steps.

            var postCreateNewActivityUrl = "https://api.fitbit.com/1/user/-/activities.json";

            using (var client = new HttpClient())
            {

                var steps = Steps.ToString();

                var today = ActivityDate.ToString("yyyy-MM-dd");
                var postData = new List<KeyValuePair<string, string>>(){

                            //17190 - walking 3.0 mph pace
                            new KeyValuePair<string, string>("activityId", "17190"),
                            new KeyValuePair<string, string>("startTime", "12:00"),
                            new KeyValuePair<string, string>("durationMillis", "43199999"),
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

                string jf = string.Empty;

                if (responseResult.StatusCode != System.Net.HttpStatusCode.OK && responseResult.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    //attempt to deserialize error state
                    try
                    {

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
                    var serializedresult = JsonSerializer.Deserialize<ActivityLogRoot>(result);
                    string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });

                }

                //*Json Can be Deserialied and then Serialized again in order to format with with indents for readabiility
                // var serializedresult = JsonSerializer.Deserialize<Root>(result);
                // string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });
                // var steps = serializedresult.summary.steps;

                //*Uncomment below lineto get result returned from API
                //return new ContentResult { Content = result, ContentType = "application/json" };
                if (responseResult.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    //new activity record created
                    SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";

                }
                else
                {
                    //System.Net.HttpStatusCode.OK

                    //no data changed -> not sure why this happens, it could be that the activity overlaps with an existing activity.
                    SystemMessage = $"No Steps Added. It is possible this activity overlaps with an activity that already exists.";

                }


                telemetry.TrackEvent("Steps Added", new Dictionary<string,string>(){{"steps",Steps.ToString()},{"ActivityDate",this.ActivityDate.ToShortDateString()}});
                SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";


                //Update Step Data
                GetStepData();

            }


        }

        /// <summary>
        /// Get All Activities by day
        /// </summary>
        /// <returns></returns>
        public ContentResult OnPostDailyActivities()
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



        /// <summary>
        ///Returns The Steps for the last 7 days as a time Series 
        /// </summary>
        public void OnPostGetPrevious7DaysSteps()
        {

            var today = DateTime.Now.ToString("yyyy-MM-dd");

            var getStepsUrl = $"https://api.fitbit.com/1/user/-/activities/steps/date/{today}/7d.json";

            using (var client = new HttpClient())
            {

                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(getStepsUrl);

                request.Headers.Add("Authorization", $"Bearer {this.Authorization.access_token}");

                var responseResult = client.SendAsync(request).GetAwaiter().GetResult();

                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var serializedresult = JsonSerializer.Deserialize<ActivitiesStepRoot>(result);

                //use json to caluclate the total steps we have done in the last 7 days, as well as the average steps per day we have completed.
                var averageStepsPerDay = serializedresult.ActivitiesSteps.Average(x => long.Parse(x.value));
                var totalStepsLast7Days = serializedresult.ActivitiesSteps.Sum(x => long.Parse(x.value));

                var content = $"{averageStepsPerDay} <<< {totalStepsLast7Days}";

                //BELOW USED FOR DEBUGGING JSON
                // string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });
                // return new ContentResult { Content = content , ContentType = "application/json" };


            }
        }

        //This method get's step data for the current logged in user, and is not triggered from a post or get
        public void GetStepData()
        {

            var today = DateTime.Now.ToString("yyyy-MM-dd");

            using (var client = new HttpClient())
            {

                var request = new HttpRequestMessage()
                {

                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://api.fitbit.com/1/user/-/activities/steps/date/{today}/7d.json")

                };


                request.Headers.Add("Authorization", $"Bearer {this.Authorization.access_token}");
                var responseResult = client.SendAsync(request).GetAwaiter().GetResult();
                var result = responseResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var serializedresult = JsonSerializer.Deserialize<ActivitiesStepRoot>(result);

                //use json to caluclate the total steps we have done in the last 7 days, as well as the average steps per day we have completed.
                var averageStepsPerDay = serializedresult.ActivitiesSteps.Average(x => long.Parse(x.value));
                var totalStepsLast7Days = serializedresult.ActivitiesSteps.Sum(x => long.Parse(x.value));

                this.LastSevenDaysAverageSteps = (long)averageStepsPerDay;
                this.LastSevenDaysSteps = totalStepsLast7Days;
            }



        }


    }
}
















