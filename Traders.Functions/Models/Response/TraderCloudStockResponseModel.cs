using System;

namespace Traders.Functions.Models.Response
{
    public class TraderCloudStockResponseModel
    {
        public Guid Id { get; set; }
        public Guid MineId { get; set; }
        public int Stock { get; set; }

        public TraderCloudStockResponseModel()
        {
        }

        public TraderCloudStockResponseModel(Guid traderId, Guid mineId, int stock)
        {
            Id = traderId;
            MineId = mineId;
            Stock = stock;
        }
    }
}