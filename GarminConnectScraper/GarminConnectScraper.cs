﻿using System.Diagnostics;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Garmin_To_FitBit_Steps_Sync_Web;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;

namespace GarminConnectScraper;

public class GarminConnectScraper
{
    private TelemetryClient telemetry;

    private readonly IConfigurationRoot _configuration;
    private long Steps { get; set; } = 0;

    public GarminConnectScraper(IConfigurationRoot cnfg)
    {
        _configuration = cnfg;
    }

    public async Task Run()
    {
        try
        {

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

        //Create steps for previous day as we want to spread our steps out over a 12 hour span, so a max of two such activities can be created in a 24 hour period.
        //If we try to submit steps on the day they occur, we will get errors when trying to submit the remaining steps later.
        var yesterday = DateTime.Now.AddDays(-1);
        var api = await FitBitAPI.InitializeApi(_configuration);

        var activityExists = await api.ActivityExistsForDate(DateOnly.FromDateTime(yesterday.Date));
        if (activityExists)
        {
            Debug.WriteLine($"Activity already exists for {yesterday.Date.ToShortDateString()}");
        }

        await api.CreateDailySteps(yesterday, Steps);

        if (api.ErrorFlag)
        {
            Debug.WriteLine($"Error:{api.ErrorMessage}");
            Console.WriteLine($"Error:{api.ErrorMessage}");
        }
        else
        {
            //telemetry.TrackEvent("Steps Added", new Dictionary<string, string>() { { "steps", Steps.ToString() }, { "ActivityDate", this.ActivityDate.ToShortDateString() } });
            //SystemMessage = $"{Steps} Steps added for {this.ActivityDate.ToShortDateString()} ";
            //GetStepData();
        }



    

       // await api.CreateDailySteps(new DateTime(yesterday.Year, yesterday.Month, yesterday.Day), Steps);

    }
}