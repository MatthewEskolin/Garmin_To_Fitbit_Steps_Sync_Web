using Microsoft.AspNetCore.Mvc;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects
{
    //TODO - learn how to use anti-forgery token + how to encrypt hidden fields (looking for a web-forms like viewstate!?)

    public class AuthorizationResponse
    {
        [FromForm]
       public string access_token {get; set;} 
       public int expires_in {get; set;}

       public string refresh_token {get; set;}

       public string token_type {get; set;}

       public string user_id {get; set;}

       public string scope { get; set; }

    }
}
