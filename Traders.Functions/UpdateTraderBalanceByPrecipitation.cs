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
using Traders.Functions.Models.Response;
using System.Collections.Generic;

namespace Traders.Functions
{
    public class UpdateTraderBalanceByPrecipitation
    {
        private readonly ITradersApiClient tradersApiClient;

        public UpdateTraderBalanceByPrecipitation(ITradersApiClient apiClient)
        {
            tradersApiClient = apiClient;
        }

        [FunctionName("UpdateTraderBalanceByDailyPrecipitation")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "weather")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("UpdateTraderBalanceByDailyPrecipitation function processed a request.");

            TraderMineRevenueRequestModel requestBody;

            try
            {
                requestBody = await req.ReadAsJson<TraderMineRevenueRequestModel>();
            }
            catch (Exception ex)
            {
                log.LogInformation(ex, "UpdateTraderBalanceByDailyPrecipitation ended due exception when deserialising request body");
                return new BadRequestObjectResult("Request body was not in the expected format");
            }

            log.LogInformation($"Request to process daily precipitation of {requestBody.Precipitation} for stocks in mine id {requestBody.MineId}");
            if (requestBody.MineId == Guid.Empty)
            {
                log.LogInformation("UpdateTraderBalanceByDailyPrecipitation ended due to empty mine id in request body");
                return new BadRequestObjectResult("Request body was not in the expected format");
            }

            if (requestBody.Precipitation <= 0)
            {
                return new OkObjectResult($"Precipitation for mine {requestBody.MineId} was not greater than zero.");
            }

            List<TraderCloudStockResponseModel> traders;

            log.LogInformation($"Fetching traders for mine: {requestBody.MineId}");
            try
            {
                var getTraders = await tradersApiClient.GetTraders(requestBody.MineId);
                traders = getTraders.Traders;
            }
            catch (Exception ex)
            {
                log.LogInformation(ex, "UpdateTraderBalanceByDailyPrecipitation ended due exception when getting traders");
                return new StatusCodeResult(500);
            }

            if ((traders?.Count ?? 0) == 0)
            {
                return new OkObjectResult($"No traders found with stock in mine id: {requestBody.MineId}");
            }

            log.LogInformation("Updating each trader's balance");
            List<TraderResponseModel> updatedTraders = new List<TraderResponseModel>();
            foreach (var trader in traders)
            {
                var revenue = CalculateDailyMineRevenue(trader.Stock, requestBody.Precipitation);
                log.LogInformation($"Updating trader {trader.Id} with daily revenue of {revenue} for mine {requestBody.MineId}");
                var updatedTrader = await tradersApiClient.PatchTraderBalance(trader.Id, revenue);
                updatedTraders.Add(updatedTrader);
            }
            return new OkObjectResult($"Trader balances updated: {updatedTraders.ToJson()}");
        }

        private static int CalculateDailyMineRevenue(int stock, int precipitation)
        {
            return stock * precipitation;
        }
    }
}