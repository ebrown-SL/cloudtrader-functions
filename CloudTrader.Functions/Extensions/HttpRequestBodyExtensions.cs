﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace CloudTrader.Functions.Extensions
{
    public static class HttpRequestBodyExtensions
    {
        public static async Task<T> ReadAsJson<T>(this HttpRequest req)
        {
            using var streamReader = new StreamReader(req.Body);
            return JsonConvert.DeserializeObject<T>(
                await streamReader.ReadToEndAsync()
            );
        }
    }
}