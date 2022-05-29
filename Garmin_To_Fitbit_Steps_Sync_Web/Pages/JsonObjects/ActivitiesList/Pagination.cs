namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivitiesList;

public class Pagination
{
    public string afterDate { get; set; }
    public int limit { get; set; }
    public string next { get; set; }
    public int offset { get; set; }
    public string previous { get; set; }
    public string sort { get; set; }
}