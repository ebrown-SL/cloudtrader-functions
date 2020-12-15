using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CloudTrader.Functions.Extensions;
using CloudTrader.Functions.Models.Weather;

namespace CloudTrader.Functions.Services
{
    public class GetAllWeather
    {
        private static string weatherUrl = Environment.GetEnvironmentVariable("WEATHER_API_URL");

        public async Task<IReadOnlyDictionary<Guid, WeatherDatum>> GetAllWeatherData(IReadOnlyDictionary<string, string> allMinesDictionary)
        {
            using var client = new HttpClient();

            var response = await client.PostAsync($"{weatherUrl}/externalweather/all/current/", allMinesDictionary.ToJsonStringContent());

            return await response.ReadAsJson<Dictionary<Guid, WeatherDatum>>();
        }
    }
}