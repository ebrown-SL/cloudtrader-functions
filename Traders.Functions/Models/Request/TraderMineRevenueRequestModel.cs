using System;

namespace Traders.Functions.Models.Request
{
    public class TraderMineRevenueRequestModel
    {
        public Guid MineId;

        public int Precipitation;

        public TraderMineRevenueRequestModel()
        {
        }

        public TraderMineRevenueRequestModel(Guid mineId, int precipitation)
        {
            MineId = mineId;

            Precipitation = precipitation;
        }
    }
}