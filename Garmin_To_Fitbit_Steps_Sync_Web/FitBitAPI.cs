using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using static System.String;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Garmin_To_FitBit_Steps_Sync_Web
{
    /// <summary>
    /// Interface for accessing FitBitAPI
    /// </summary>
    public class FitBitAPI
    {
        private readonly IConfigurationRoot _config;
        private bool _tokensVerified = false;

        public static async Task<FitBitAPI> InitializeApi(IConfigurationRoot config, bool updateTokens = true)
        {
            var api = new FitBitAPI(config);

            if (updateTokens)
            {
                var updateTokensResult = await api.UpdateTokens();

                if (updateTokensResult == false)
                {
                    throw new Exception("Failed to Update Tokens");
                }
            }

            return api;
        }


        private FitBitAPI(IConfigurationRoot config)
        {
            _config = config;

            //check if api is ready - if yes set _tokensVerfied to true, else report token error


            //Console.WriteLine("Could not get valid access token...");


        }

        


        /// <summary>
        /// Update the KeyVault with new tokens
        /// </summary>
        /// <returns></returns>
        ///
        public  async Task<Response<KeyVaultSecret>> UpdateKeyVault(string key, string value)
        {
            var keyVaultUriString = $"https://{_config["KeyVaultName"]}.vault.azure.net/";
            var keyVaultUri = new System.Uri(keyVaultUriString);

            var credential = new DefaultAzureCredential();

            var client = new SecretClient(keyVaultUri, credential);

            var result = await client.SetSecretAsync(new KeyVaultSecret(key, value));

            return result;

        }

        private async Task<bool> UpdateTokens()
        {
            Debug.Assert(!IsNullOrEmpty(_config["Fitbit:AccessToken"]));
            Debug.Assert(!IsNullOrEmpty(_config["Fitbit:RefreshToken"]));

            //query access token from key vault;out access token
            var accessToken = _config["Fitbit:AccessToken"];
            var refreshToken = _config["Fitbit:RefreshToken"];

            var currentAuthInfo = new FitBitAuthInfo(refreshToken, accessToken);

            Debug.WriteLine($"Current Access Token: {currentAuthInfo.AccessToken}");
            Debug.WriteLine($"Current Refresh Token: {currentAuthInfo.RefreshToken}");

            var newAuthInfo = await GetNewTokens(currentAuthInfo);

            if (newAuthInfo.Item1 == false)
            {
                Debug.WriteLine("Request to FitBitAPI for new tokens has failed");
                return false;
            }

            //TODO named tuples
            Debug.WriteLine("");
            Debug.WriteLine($"New Access Token: {newAuthInfo.Item2.AccessToken}");
            Debug.WriteLine($"New Refresh Token: {newAuthInfo.Item2.RefreshToken}");

            //TODO convert to named item tuple
            AuthorizationInfo = newAuthInfo.Item2;

            //Update KeyVault
            var u1 = UpdateKeyVault("Fitbit--AccessToken", AuthorizationInfo.AccessToken);
            var u2 = UpdateKeyVault("Fitbit--RefreshToken", AuthorizationInfo.RefreshToken);

            await u1;
            await u2;

            _config.Reload();
            return true;

        }

        private FitBitAuthInfo AuthorizationInfo { get; set; }


        public bool ErrorFlag { get; set; }
        public string ErrorMessage { get; set; } = Empty;

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
        /// Helper method to call the FitBit API and processes the Response
        /// </summary>
        /// <param name="authInfo"></param>
        /// <returns></returns>
        private async Task<(bool, FitBitAuthInfo)> GetNewTokens(FitBitAuthInfo authInfo)
        {
            //Make Request to FitBit API for new tokens
            HttpResponseMessage requestResult = await RequestNewTokens(authInfo);
            var resultString = await requestResult.Content.ReadAsStringAsync();

            //Check for Success
            if (requestResult.StatusCode != HttpStatusCode.OK)
            {
                //request has failed return false
                return (false, null);
            }

            
            //parse the Json to extract the new refresh token 
            var authResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(resultString);
            if (authResponse == null)
            {
                return (false, null);
            }

            ////return updated tokens
            var authinfo = new FitBitAuthInfo(authResponse.refresh_token, authResponse.access_token);

            Debug.Write(resultString);

            return (true, authinfo);
        }


        /// <summary>
        /// Fitbit API Call - /oauth2/token
        /// </summary>
        /// <returns></returns>
        [FitBitApiMethod]
        private async Task<HttpResponseMessage> RequestNewTokens(FitBitAuthInfo authInfo)
        {
            var url = "https://api.fitbit.com/oauth2/token";

            using var client = GetHttpClient();

            var postData = new List<KeyValuePair<string, string>>(){

                new("refresh_token", $"{authInfo.RefreshToken}"),
                new("grant_type", $"refresh_token"),
                new("expires_in", "28800")
            };

            var content = new FormUrlEncodedContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;

            request.Headers.Add("Authorization", $"Bearer {authInfo.AccessToken}");


            var responseResult = await client.SendAsync(request);

            return responseResult;
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
            ErrorMessage = Empty;
        }

        private void SetError(string errorMsg)
        {
            ErrorFlag = true;
            ErrorMessage = errorMsg;

        }


        /// <summary>
        /// Fitbit API Call - /1/user/-/profile.json
        /// </summary>
        /// <returns></returns>
        [FitBitApiMethod]
        public async Task<HttpResponseMessage> GetUserProfile()
        {
            var url = "https://api.fitbit.com/1/user/-/profile.json";

            using var client = GetHttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Authorization", $"Bearer {this.AuthorizationInfo.AccessToken}");

            var responseResult = await client.SendAsync(request);

            return responseResult;

        }
    }
}
