using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Traders.Functions.Helpers
{
    public static class HttpContentExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
            throw new JsonSerializationException("JSON serialization error");
        }

        public static HttpContent ToJsonStringContent(this object obj)
        {
            return new StringContent(
                obj.ToJson(),
                Encoding.UTF8,
                "application/json"
            );
        }
    }
}