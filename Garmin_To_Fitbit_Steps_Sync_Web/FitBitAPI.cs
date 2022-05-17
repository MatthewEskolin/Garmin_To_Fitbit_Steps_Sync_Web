using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Garmin_To_FitBit_Steps_Sync_Web
{
    public class FitBitAPI
    {
        private readonly IConfigurationRoot _config;
        private bool _tokensVerified = false;

        public FitBitAPI(IConfigurationRoot config)
        {
            _config = config;
        }

        private async Task UpdateTokens()
        {
            Debug.Assert(!String.IsNullOrEmpty(_config["Fitbit:AccessToken"]));
            Debug.Assert(!String.IsNullOrEmpty(_config["Fitbit:RefreshToken"]));

            //query access token from key vault;out access token
            var accessToken = _config["Fitbit:AccessToken"];
            var refreshToken = _config["Fitbit:RefreshToken"];

            var currentAuthInfo = new FitBitAuthInfo(refreshToken, accessToken);
            var newAuthInfo = await RefreshTokens(currentAuthInfo);

            AuthorizationInfo = newAuthInfo;
            //Make a request to / to 
        }

        public FitBitAuthInfo AuthorizationInfo { get; set; }



        public bool ErrorFlag { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Fitbit API Call - /1/user/-/activities.json 
        /// </summary>
        /// <param name="activityDate"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        [FitBitApiMethod]
        public async Task CreateDailySteps(DateTime activityDate, long steps)
        {
            ResetError();

            //Create's an activity inside my fitbit account for the day with the input number of steps.

            string postCreateNewActivityUrl = "https://api.fitbit.com/1/user/-/activities.json";

            using var client = GetHttpClient();

    
            var today = activityDate.ToString("yyyy-MM-dd");
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


            HttpContent content = new FormUrlEncodedContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(postCreateNewActivityUrl),
                Content = content

            };

            request.Headers.Add("Authorization", $"Bearer {this.AuthorizationInfo.AccessToken}");

            Debug.WriteLine($"Sending Request to FitBit {request.RequestUri}");


            var responseResult = await client.SendAsync(request);
            var result = await responseResult.Content.ReadAsStringAsync();

            switch (responseResult.StatusCode)
            {
                case HttpStatusCode.Created:

                    Debug.WriteLine("Activity Created Success!");
                    Debug.WriteLine("");

                    //Should be an activity root returned
                    var serializedresult1 = JsonSerializer.Deserialize<ActivityLogRoot>(result);
                    string jsonFormatted = JsonSerializer.Serialize(serializedresult1, new JsonSerializerOptions() { WriteIndented = true });
                    break;

                case HttpStatusCode.OK:
                    break;
                default:
                    Debug.WriteLine($"Fitbit API Error Response");

                    //Error 
                    //attempt to deserialize error state to get error message
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
                    break;
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


                //telemetry.TrackEvent("Steps Added", new Dictionary<string, string>() { { "steps", Steps.ToString() }, { "ActivityDate", this.ActivityDate.ToShortDateString() } });
            //    SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";




        }

        /// <summary>
        /// Fitbit API Call - /oauth2/token
        /// </summary>
        /// <returns></returns>
        [FitBitApiMethod]
        public async Task<FitBitAuthInfo> RefreshTokens(FitBitAuthInfo authInfo)
        {
            var url = "https://api.fitbit.com/oauth2/token";

            var options = new RestClientOptions(url)
            {
                Timeout = 90
            };

            var client = new RestClient(options);
            var request = new RestRequest(url, Method.Post);

            request.AddHeader("Authorization", $"Bearer {authInfo.AccessToken}");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", $"{authInfo.RefreshToken}");
            request.AddParameter("expires_in", "28800");

            RestResponse response = await client.ExecuteAsync(request);

            //return updated tokens
            var authinfo = new FitBitAuthInfo("0", "0");


            Debug.Write(response.Content);
            Console.WriteLine(response.Content);

            return authinfo;
        }



        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            return client;
        }


        //

        

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
