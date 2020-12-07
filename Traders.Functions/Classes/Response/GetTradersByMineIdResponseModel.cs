using System.Collections.Generic;

namespace Traders.Functions.Classes.Response
{
    public class GetTradersByMineIdResponseModel
    {
        public List<TraderCloudStockResponseModel> Traders { get; set; }

        public GetTradersByMineIdResponseModel()
        {
        }

        public GetTradersByMineIdResponseModel(List<TraderCloudStockResponseModel> traders)
        {
            Traders = traders;
        }
    }
}