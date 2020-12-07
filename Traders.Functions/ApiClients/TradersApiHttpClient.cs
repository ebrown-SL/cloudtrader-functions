using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Traders.Functions.Classes.Request;
using Traders.Functions.Classes.Response;
using Traders.Functions.Helpers;

namespace Traders.Functions.ApiClients
{
    public class TradersApiHttpClient : ITradersApiClient
    {
        private HttpClient client;

        // TO DO: this needs to be env var
        private string tradersUrl = Environment.GetEnvironmentVariable("TRADER_API_URL");

        public TradersApiHttpClient(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient();
        }

        public async Task<GetTradersByMineIdResponseModel> GetTraders(Guid mineId)
        {
            try
            {
                var resp = await client.GetAsync($"{tradersUrl}/mines/{mineId}");
                resp.EnsureSuccessStatusCode();
                return await resp.ReadAsJson<GetTradersByMineIdResponseModel>();
            }
            catch
            {
                throw new Exception($"Error getting all traders by mine id: {mineId}");
            }
        }

        public async Task<TraderResponseModel> PatchTraderBalance(Guid traderId, int amountToAdd)
        {
            var content = new UpdateTraderBalanceRequestModel() { AmountToAdd = amountToAdd }.ToJsonStringContent();
            try
            {
                var resp = await client.PatchAsync($"{tradersUrl}/{traderId}/balance", content);
                resp.EnsureSuccessStatusCode();
                return await resp.ReadAsJson<TraderResponseModel>();
            }
            catch
            {
                throw new Exception($"Error patching balance for trader id: {traderId}");
            }
        }
    }
}