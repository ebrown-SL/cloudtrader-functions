using System;
using System.Threading.Tasks;
using Traders.Functions.Classes.Response;

namespace Traders.Functions.ApiClients
{
    public interface ITradersApiClient
    {
        Task<GetTradersByMineIdResponseModel> GetTraders(Guid mineId);

        Task<TraderResponseModel> PatchTraderBalance(Guid traderId, int amountToAdd);
    }
}