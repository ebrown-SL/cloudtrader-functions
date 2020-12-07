using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Traders.Functions.Helpers;
using Traders.Functions.ApiClients;
using System;

namespace Traders.Functions
{
    public class UpdateTraderBalanceFromDailyPrecipitation
    {
        private readonly ITradersApiClient tradersApiClient;

        public UpdateTraderBalanceFromDailyPrecipitation(ITradersApiClient apiClient)
        {
            tradersApiClient = apiClient;
        }

        [FunctionName("UpdateTraderBalanceFromDailyPrecipitation")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "/balance")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("MineRevenue function processed a request.");

            TraderMineRevenueRequestModel requestBody = await req.ReadAsJson<TraderMineRevenueRequestModel>();
            if (requestBody.MineId == null)
            {
                throw new Exception("Mine Id must be provided");
            }

            if (requestBody.Precipitation == 0)
            {
                return new OkObjectResult($"Precipitation for mine {requestBody.MineId} was 0.");
            }

            log.LogInformation($"Fetching traders for mine: {requestBody.MineId}");
            var getTraders = await tradersApiClient.GetTraders(requestBody.MineId);
            var traders = getTraders.Traders;

            log.LogInformation("Updating each traders balance");
            foreach (var trader in traders)
            {
                var revenue = CalculateDailyMineRevenue(trader.Stock, requestBody.Precipitation);
                log.LogInformation($"Updating trader {trader.Id} with daily revenue of {revenue}");
                await tradersApiClient.PatchTraderBalance(trader.Id, revenue);
            }
            // TO DO: check returned balance is as expected, then send ok (or log error)
            return new OkObjectResult($"all traders: {traders.ToJson()}");
        }

        private static int CalculateDailyMineRevenue(int stock, int precipitation)
        {
            return stock * precipitation;
        }
    }
}