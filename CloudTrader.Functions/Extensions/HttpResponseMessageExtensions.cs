using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudTrader.Functions.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> ReadAsJson<T>(this HttpResponseMessage message)
        {
            return JsonConvert.DeserializeObject<T>(
                await message.Content.ReadAsStringAsync()
            );
            throw new JsonException("Error converting Http response message to JSON.");
        }
    }
}