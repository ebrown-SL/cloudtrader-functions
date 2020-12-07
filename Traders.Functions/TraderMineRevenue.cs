using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Traders.Functions.Helpers;
using Traders.Functions.Classes.Response;
using Traders.Functions.Classes.Request;
using System;

namespace Traders.Functions
{
    public static class TraderMineRevenue
    {
        [FunctionName("TraderMineRevenue")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mine/revenue")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("MineRevenue function processed a request.");
            TraderMineRevenueRequestModel requestBody = await req.ReadAsJson<TraderMineRevenueRequestModel>();

            if (requestBody.Precipitation == 0)
            {
                return new OkObjectResult($"Precipitation for mine {requestBody.MineId} was 0.");
            }

            var client = new HttpClient();
            // TO DO: this needs to be env var
            string tradersUri = "http://localhost:5999/api/trader";

            log.LogInformation($"Fetching traders for mine: {requestBody.MineId}");
            var getTraders = await GetTraders(client, tradersUri, requestBody.MineId);
            var traders = getTraders.Traders;

            log.LogInformation("Updating each traders balance");
            foreach (var trader in traders)
            {
                var revenue = CalculateRevenue(trader.Stock, requestBody.Precipitation);
                log.LogInformation($"Updating trader {trader.Id} with daily revenue of {revenue}");
                var content = new UpdateTraderBalanceRequestModel() { AmountToAdd = revenue }.ToJsonStringContent();
                await PatchTraderBalance(client, tradersUri, trader, content);
            }
            // TO DO: check returned balance is as expected, then send ok (or log error)
            return new OkObjectResult($"all traders: {traders.ToJson()}");
        }

        private static async Task<GetTradersByMineIdResponseModel> GetTraders(HttpClient client, string tradersUri, Guid mineId)
        {
            var resp = await client.GetAsync($"{tradersUri}/mines/{mineId}");
            return await resp.ReadAsJson<GetTradersByMineIdResponseModel>();
        }

        private static async Task<TraderResponseModel> PatchTraderBalance(HttpClient client, string tradersUri, TraderCloudStockResponseModel trader, HttpContent content)
        {
            var resp = await client.PatchAsync($"{tradersUri}/{trader.Id}/balance", content);
            return await resp.ReadAsJson<TraderResponseModel>();
        }

        private static int CalculateRevenue(int stock, int precipitation)
        {
            return stock * precipitation;
        }
    }
}