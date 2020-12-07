using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Traders.Functions.Helpers
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> ReadAsJson<T>(this HttpResponseMessage message)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(
                    await message.Content.ReadAsStringAsync()
                );
            }
            catch
            {
                throw new JsonException("Error converting Http response message to JSON.");
            }
        }
    }
}