using System;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivityType
{

        //*Generated class from json2csharp.com //
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Activity    {
        public int activityId { get; set; } 
        public int activityParentId { get; set; } 
        public string activityParentName { get; set; } 
        public int calories { get; set; } 
        public string description { get; set; } 
        public double distance { get; set; } 
        public int duration { get; set; } 
        public bool hasActiveZoneMinutes { get; set; } 
        public bool hasStartTime { get; set; } 
        public bool isFavorite { get; set; } 
        public DateTime lastModified { get; set; } 
        public long logId { get; set; } 
        public string name { get; set; } 
        public string startDate { get; set; } 
        public string startTime { get; set; } 
        public int steps { get; set; } 
    }
}


