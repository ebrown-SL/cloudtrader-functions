using System;
using System.Collections.Generic;
using System.Net.Http;
using CloudTrader.Functions.Extensions;
using CloudTrader.Functions.Models.Mines;
using CloudTrader.Functions.Models.Weather;

namespace CloudTrader.Functions.Services
{
    internal class SendAllWeather
    {
        private static string minesUrl = Environment.GetEnvironmentVariable("MINE_API_URL");

        public async void SendAllMinesWeather(IReadOnlyDictionary<Guid, WeatherDatum> allWeather)
        {
            foreach (KeyValuePair<Guid, WeatherDatum> entry in allWeather)
            {
                var weatherUpdateForMine = new MineUpdateModel();
                weatherUpdateForMine.Temperature = Convert.ToInt32(Math.Round(entry.Value.Temperature));
                weatherUpdateForMine.Stock = Convert.ToInt32(Math.Round(entry.Value.Clouds));
                weatherUpdateForMine.UpdateType = UpdateType.weather;
                weatherUpdateForMine.Time = DateTime.Now;

                using var client = new HttpClient();

                var response = await client.PatchAsync($"{minesUrl}/api/mine/{entry.Key}", weatherUpdateForMine.ToJsonStringContent());
            }
        }
    }
}