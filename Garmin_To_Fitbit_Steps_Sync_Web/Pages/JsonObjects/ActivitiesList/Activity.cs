using System;
using System.Collections.Generic;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Pages.JsonObjects.ActivitiesList;

public class Activity
{
    public int activeDuration { get; set; }
    public List<ActivityLevel> activityLevel { get; set; }
    public string activityName { get; set; }
    public int activityTypeId { get; set; }
    public int calories { get; set; }
    public double distance { get; set; }
    public string distanceUnit { get; set; }
    public int duration { get; set; }
    public bool hasActiveZoneMinutes { get; set; }
    public DateTime lastModified { get; set; }
    public long logId { get; set; }
    public string logType { get; set; }
    public ManualValuesSpecified manualValuesSpecified { get; set; }
    public int originalDuration { get; set; }
    public DateTime originalStartTime { get; set; }
    public double pace { get; set; }
    public Source source { get; set; }
    public double speed { get; set; }
    public DateTime startTime { get; set; }
    public int steps { get; set; }
}