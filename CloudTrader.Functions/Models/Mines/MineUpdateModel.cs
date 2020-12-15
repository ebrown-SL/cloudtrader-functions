using System;
using System.ComponentModel.DataAnnotations;

namespace CloudTrader.Functions.Models.Mines
{
    public class MineUpdateModel
    {
        private UpdateType updateType;

        public double? Temperature { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        public string? Name { get; set; }

        public UpdateType UpdateType
        {
            get => updateType;
            set
            {
                if (!(value == UpdateType.trade || value == UpdateType.weather))
                {
                    throw new ArgumentException("UpdateType is invalid");
                }
                else
                {
                    updateType = value;
                }
            }
        }

        public DateTime Time { get; set; }
    }
}