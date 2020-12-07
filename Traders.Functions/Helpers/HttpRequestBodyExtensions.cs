using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Traders.Functions.Helpers
{
    public static class HttpRequestBodyExtensions
    {
        public static async Task<T> ReadAsJson<T>(this HttpRequest req)
        {
            using var streamReader = new StreamReader(req.Body);
            try
            {
                return JsonConvert.DeserializeObject<T>(
                    await streamReader.ReadToEndAsync()
                );
            }
            catch
            {
                throw new JsonException("Error converting Http request stream to JSON.");
            }
        }
    }
}