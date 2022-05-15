using System.Diagnostics;
using Garmin_To_Fitbit_Steps_Sync_Web;
using Microsoft.Extensions.Configuration;

namespace GarminConnectScraper;

public class GarminConnectScraper
{
    private readonly IConfigurationRoot _configuration;
    private long Steps { get; set; } = 0;

    public GarminConnectScraper(IConfigurationRoot cnfg)
    {
        _configuration = cnfg;
    }

    public async Task Run()
    {
        var steps = await GetStepsFromGarmin();

        Steps = steps;

        await SendStepsToFitBit();
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
        var api = new FitBitAPI(_configuration);

        //Create steps for previous day as we want to spread our steps out over a 12 hour span, so a max of two such activities can be created in a 24 hour period.
        //If we try to submit steps on the day they occur, we will get errors when trying to submit the remaining steps later.
        var yesterday = DateTime.Now.AddDays(-1);

        Debug.WriteLine($"Sending to FitBit...");
        await api.CreateDailySteps(new DateTime(yesterday.Year, yesterday.Month, yesterday.Day), Steps);

        if (api.ErrorFlag)
        {
            Debug.WriteLine($"Error:{api.ErrorMessage}");
            Console.WriteLine($"Error:{api.ErrorMessage}");
        }
    }
}