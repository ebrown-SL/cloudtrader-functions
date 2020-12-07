using System;
using System.Collections.Generic;
using System.Text;

namespace Traders.Functions
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