using System;

namespace Traders.Functions.Models.Response
{
    public class TraderResponseModel
    {
        public Guid Id { get; set; }
        public int Balance { get; set; }

        public TraderResponseModel()
        {
        }

        public TraderResponseModel(Guid id, int balance)
        {
            Id = id;
            Balance = balance;
        }
    }
}