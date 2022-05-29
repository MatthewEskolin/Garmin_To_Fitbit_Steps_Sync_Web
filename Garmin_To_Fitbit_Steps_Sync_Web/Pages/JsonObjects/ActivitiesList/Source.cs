using System.Collections;
using System.Collections.Generic;


namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivitiesList
{
    public class Source
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<object> trackerFeatures { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }
}
