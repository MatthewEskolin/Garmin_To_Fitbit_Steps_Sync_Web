namespace Garmin_To_Fitbit_Steps_Sync_Web;

public class FitBitAuthInfo
{
    public FitBitAuthInfo(string refresh_token, string access_token)
    {
        this.RefreshToken = refresh_token;
        this.AccessToken = access_token;
    }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}