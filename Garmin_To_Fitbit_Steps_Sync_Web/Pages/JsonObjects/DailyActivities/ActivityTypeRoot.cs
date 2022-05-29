using System.Collections.Generic;
using Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivityType;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.DailyActivities
{

    public class ActivityTypeRoot    {
        public List<Activity> activities { get; set; } 
        public Goals goals { get; set; } 
        public Summary summary { get; set; } 
    
}

}

