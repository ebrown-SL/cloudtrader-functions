using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Traders.Functions.Helpers
{
    public static class HttpContentExtensions
    {
        public static string ToJson(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch
            {
                throw new JsonSerializationException("JSON serialization error");
            }
        }

        public static HttpContent ToJsonStringContent(this object obj)
        {
            try
            {
                return new StringContent(
                    obj.ToJson(),
                    Encoding.UTF8,
                    "application/json"
                );
            }
            catch
            {
                throw new JsonException("Error converting JSON to Http string content");
            }
        }
    }
}