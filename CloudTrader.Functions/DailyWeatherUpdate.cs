using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using CloudTrader.Functions.Services;

namespace CloudTrader.Functions
{
    public static class DailyWeatherUpdate
    {
        [FunctionName("DailyWeatherUpdate")]
        public static async Task Run([TimerTrigger("0 0 8 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var gAM = new GetAllMines();
            log.LogInformation($"Get all the mines");
            var minesDict = await gAM.GetAllMinesDict();

            var gAW = new GetAllWeather();
            log.LogInformation($"Get all the weather");
            var weatherData = await gAW.GetAllWeatherData(minesDict);

            var sAW = new SendAllWeather();
            log.LogInformation($"Send all the weather");
            sAW.SendAllMinesWeather(weatherData);
        }
    }
}