using System;
using System.Threading.Tasks;
using CloudTrader.Functions.Traders.Models.Response;

namespace CloudTrader.Functions.ApiClients
{
    public interface ITradersApiClient
    {
        Task<GetTradersByMineIdResponseModel> GetTraders(Guid mineId);

        Task<TraderResponseModel> PatchTraderBalance(Guid traderId, int amountToAdd);
    }
}