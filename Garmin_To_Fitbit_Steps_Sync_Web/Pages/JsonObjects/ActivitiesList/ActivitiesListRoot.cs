using System.Collections.Generic;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivitiesList;

public class ActivitiesListRoot
{
    public List<Activity> activities { get; set; }
    public Pagination pagination { get; set; }
}