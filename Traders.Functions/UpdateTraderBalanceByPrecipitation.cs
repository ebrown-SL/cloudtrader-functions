using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Traders.Functions.Helpers;
using Traders.Functions.ApiClients;
using System;
using Traders.Functions.Models.Request;

namespace Traders.Functions
{
    public class UpdateTraderBalanceByPrecipitation
    {
        private readonly ITradersApiClient tradersApiClient;

        public UpdateTraderBalanceByPrecipitation(ITradersApiClient apiClient)
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
                throw new ArgumentNullException("Mine Id must be provided");
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
                log.LogInformation($"Updating trader {trader.Id} with daily revenue of {revenue} from mine {requestBody.MineId}");
                await tradersApiClient.PatchTraderBalance(trader.Id, revenue);
            }
            return new OkObjectResult($"Traders updated: {traders.ToJson()}");
        }

        private static int CalculateDailyMineRevenue(int stock, int precipitation)
        {
            return stock * precipitation;
        }
    }
}