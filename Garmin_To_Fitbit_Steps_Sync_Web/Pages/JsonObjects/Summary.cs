using System.Collections.Generic;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects
{

        public class Summary    {
        public int activeScore { get; set; } 
        public int activityCalories { get; set; } 
        public int caloriesBMR { get; set; } 
        public int caloriesOut { get; set; } 
        public List<Distance> distances { get; set; } 
        public int fairlyActiveMinutes { get; set; } 
        public int lightlyActiveMinutes { get; set; } 
        public int marginalCalories { get; set; } 
        public int sedentaryMinutes { get; set; } 
        public int steps { get; set; } 
        public int veryActiveMinutes { get; set; } 
    
}
}