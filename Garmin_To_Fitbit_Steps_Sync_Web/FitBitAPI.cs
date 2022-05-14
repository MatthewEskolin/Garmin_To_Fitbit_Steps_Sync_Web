using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects;
using Microsoft.AspNetCore.Http;

namespace Garmin_To_Fitbit_Steps_Sync_Web
{
    public class FitBitAPI
    {
        public FitBitAPI(AuthorizationResponse auth)
        {
        }

        public AuthorizationResponse AuthorizationInfo { get; set; }


        public bool ErrorFlag { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;


        public async void CreateDailySteps(DateTime activitydate, long steps)
        {

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

            request.Headers.Add("Authorization", $"Bearer {this.AuthorizationInfo.access_token}");

            var responseResult = await client.SendAsync(request);//.GetAwaiter().GetResult();

            var result = await responseResult.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();

            string jf = string.Empty;

            if (responseResult.StatusCode != System.Net.HttpStatusCode.OK && responseResult.StatusCode != System.Net.HttpStatusCode.Created)
            {
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

        private void SetError(string errorMsg)
        {
            ErrorFlag = true;
            ErrorMessage = errorMsg;

        }
    }
}
