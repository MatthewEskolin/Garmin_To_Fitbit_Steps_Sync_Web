using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Garmin_To_FitBit_Steps_Sync_Web
{
    public class FitBitAPI
    {
        private readonly IConfigurationRoot _config;

        public FitBitAPI(IConfigurationRoot config)
        {
            _config = config;
            UpdateTokens();

        }

        private void UpdateTokens()
        {
            Debug.Assert(!String.IsNullOrEmpty(_config["Fitbit:AccessToken"]));
            Debug.Assert(!String.IsNullOrEmpty(_config["Fitbit:RefreshToken"]));

            //query access token from key vault;out access token
            var accessToken = _config["Fitbit:AccessToken"];
            var refreshToken = _config["Fitbit:RefreshToken"];

            var currentAuthInfo = new FitBitAuthInfo(refreshToken, accessToken);




            AuthorizationInfo = authInfo;
            //Make a request to / to 
        }

        public FitBitAuthInfo AuthorizationInfo { get; set; }



        public bool ErrorFlag { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Fitbit API Call - /1/user/-/activities.json
        /// </summary>
        /// <param name="activitydate"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public async Task CreateDailySteps(DateTime activitydate, long steps)
        {
            ResetError();


            //TODO_REFACTOR Does it make sense to bind the dependencies for this method as a parameter.

            //Create's an activity inside my fitbit account for the day with the input number of steps.

            var postCreateNewActivityUrl = "https://api.fitbit.com/1/user/-/activities.json";

            using var client = new HttpClient();

    
                var today = activitydate.ToString("yyyy-MM-dd");
                var postData = new List<KeyValuePair<string, string>>(){

                            //17190 - walking 3.0 mph pace
                            new("activityId", "17190"),
                            //Start walking around noon
                            new("startTime", "12:00"),

                            //just under 12 hours
                            new("durationMillis", "43199999"),
                            //inserting steps for today
                            new("date", today),
                            //unit is steps
                            new("distanceUnit", "steps"),
                            //total steps here
                            new("distance", steps.ToString()),
                        };


            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(postCreateNewActivityUrl);

            HttpContent content = new FormUrlEncodedContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            request.Content = content;

            request.Headers.Add("Authorization", $"Bearer {this.AuthorizationInfo.AccessToken}");

            Debug.WriteLine($"Sending Request to FitBit {request.RequestUri}");

            HttpResponseMessage responseResult;


            responseResult = await client.SendAsync(request);



            var result = await responseResult.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();

            string jf = string.Empty;

            if (responseResult.StatusCode != System.Net.HttpStatusCode.OK && responseResult.StatusCode != System.Net.HttpStatusCode.Created)
            {
                
                Debug.WriteLine($"Fitbit API Error Response");
                //attempt to deserialize error state
                try
                {

                    var serializedresult = JsonSerializer.Deserialize<ErrorRoot>(result);


                    if (serializedresult != null)
                    {
                        SetError(serializedresult.errors[0].message);
                    }


                    return;

                }
                catch
                {
                    SetError("Activty Creation Failed - Unknown Response Type.");
                    return;

                }
            }
            else
            {
                Debug.WriteLine("Activity Created Success!");
                Debug.WriteLine("");

                //Should be an activity root returned
                var serializedresult = JsonSerializer.Deserialize<ActivityLogRoot>(result);
                string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });

            }

            //    //*Json Can be Deserialied and then Serialized again in order to format with with indents for readabiility
            //    // var serializedresult = JsonSerializer.Deserialize<Root>(result);
            //    // string jsonFormatted = JsonSerializer.Serialize(serializedresult, new JsonSerializerOptions() { WriteIndented = true });
            //    // var steps = serializedresult.summary.steps;

            //    //*Uncomment below lineto get result returned from API
            //    //return new ContentResult { Content = result, ContentType = "application/json" };
            //    if (responseResult.StatusCode == System.Net.HttpStatusCode.Created)
            //    {
            //        //new activity record created
            //        SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";

            //    }
            //    else
            //    {
            //        //System.Net.HttpStatusCode.OK

            //        //no data changed -> not sure why this happens, it could be that the activity overlaps with an existing activity.
            //        SystemMessage = $"No Steps Added. It is possible this activity overlaps with an activity that already exists.";

            //    }


            //    telemetry.TrackEvent("Steps Added", new Dictionary<string, string>() { { "steps", Steps.ToString() }, { "ActivityDate", this.ActivityDate.ToShortDateString() } });
            //    SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";




        }

       
        

        

        private void ResetError()
        {
            ErrorFlag = false;
            ErrorMessage = string.Empty;
        }

        private void SetError(string errorMsg)
        {
            ErrorFlag = true;
            ErrorMessage = errorMsg;

        }
    }
}
