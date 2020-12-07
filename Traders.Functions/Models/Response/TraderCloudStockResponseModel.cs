using System;

namespace Traders.Functions.Classes.Response
{
    public class TraderCloudStockResponseModel
    {
        public Guid Id { get; set; }
        public Guid MineId { get; set; }
        public int Stock { get; set; }
    }
}