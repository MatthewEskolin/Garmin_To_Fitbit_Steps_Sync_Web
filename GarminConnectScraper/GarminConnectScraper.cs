using System.Diagnostics;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Garmin_To_FitBit_Steps_Sync_Web;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace GarminConnectScraper;

public class GarminConnectScraper
{
    private TelemetryClient telemetry;

    private readonly IConfigurationRoot _configuration;
    private long Steps { get; set; } = 0;
    private FitBitAPI fbAPI { get; set; }
    private DateOnly Yesterday { get; set; }

    private GarminConnectScraper(IConfigurationRoot cnfg)
    {
        _configuration = cnfg;

        //Create steps for previous day as we want to spread our steps out over a 12 hour span, so a max of two such activities can be created in a 24 hour period.
        //If we try to submit steps on the day they occur, we will get errors when trying to submit the remaining steps later.
        Yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    }


    public static async Task<GarminConnectScraper> Create(IConfigurationRoot cnfg)
    {
        var gcs = new GarminConnectScraper(cnfg)
        {
            fbAPI = await FitBitAPI.InitializeApi(cnfg)
        };
        return gcs;
    }


    public async Task Run()
    {
        try
        {
            //check if steps for the previous day have already been input. If yes, refresh token and exit. 
            var stepsExist = await this.CheckIfFbStepsExist();
            if (stepsExist)
            {
                Debug.WriteLine($"Steps for {Yesterday} already in Fitbit");
                Console.WriteLine($"Steps for {Yesterday} already in Fibit");
                return;
            }

            var steps = await GetStepsFromGarmin();

            Steps = steps;

            await SendStepsToFitBit();
        }
        finally
        {
            if (this._configuration["ReadLine"] == "true")
            {
                Console.ReadLine();
            }
        }
    }


    private async Task<bool> CheckIfFbStepsExist()
    {
        var activityExists = await fbAPI.ActivityExistsForDate(Yesterday);
        return activityExists;
    }

    private async Task<int> GetStepsFromGarmin()
    {
        //Log Yesterdays Steps
        var sr = new StepReader();

        //Scrape Steps from Garmin
        var steps = await sr.GetYesterdaysSteps(_configuration);

        Debug.WriteLine($"Steps = {steps}");
        Debug.WriteLine(steps == 0 ? "Warning: Steps = 0" : $"Received Steps from Garmin...");


        return steps;
    }
    private async Task SendStepsToFitBit()
    {

        Debug.WriteLine($"Sending to FitBit...");

        var activityExists = await fbAPI.ActivityExistsForDate(Yesterday);
        if (activityExists)
        {
            Debug.WriteLine($"Activity already exists for {Yesterday.ToShortDateString()}");
            return;
        }

        await fbAPI.CreateDailySteps(Yesterday, Steps);

        if (fbAPI.ErrorFlag)
        {
            Debug.WriteLine($"Error:{fbAPI.ErrorMessage}");
            Console.WriteLine($"Error:{fbAPI.ErrorMessage}");
        }
        else
        {
            //TODO need telemetry here from dependency injection

            var tel = TelemetryConfiguration.CreateDefault();
            var client = new TelemetryClient(tel);


            //telemetry.TrackEvent("Steps Added", new Dictionary<string, string>() { { "steps", Steps.ToString() }, { "ActivityDate", this.ActivityDate.ToShortDateString() } });
            //SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";
            //GetStepData();
        }



    

       // await api.CreateDailySteps(new DateTime(yesterday.Year, yesterday.Month, yesterday.Day), Steps);

    }
}