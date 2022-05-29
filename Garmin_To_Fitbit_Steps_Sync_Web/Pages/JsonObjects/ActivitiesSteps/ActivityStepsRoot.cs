using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivitiesSteps
{

    public class ActivitiesStep    {
        public string dateTime { get; set; } 
        public string value { get; set; } 

        [JsonIgnore]
        public System.DateTime ActivityDate {
            get{
                return System.DateTime.Parse(dateTime);
            }
        }

    }

    public class ActivitiesStepRoot {

        [JsonPropertyName("activities-steps")]
        public List<ActivitiesStep> ActivitiesSteps { get; set; } 
    }


}
