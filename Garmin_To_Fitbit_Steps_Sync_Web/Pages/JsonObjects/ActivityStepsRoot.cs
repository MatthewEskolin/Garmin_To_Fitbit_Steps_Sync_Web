using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects
{

    public class ActivitiesStep    {
        public string dateTime { get; set; } 
        public string value { get; set; } 

        [JsonIgnoreAttribute]
        public System.DateTime ActivityDate {
            get{
                return System.DateTime.Parse(dateTime);
            }
        }

    }

    public class ActivitiesStepRoot {

        [JsonPropertyNameAttribute("activities-steps")]
        public List<ActivitiesStep> ActivitiesSteps { get; set; } 
    }


}
