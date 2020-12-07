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
            return JsonConvert.DeserializeObject<T>(
                await new StreamReader(req.Body).ReadToEndAsync()
            );
        }
    }
}