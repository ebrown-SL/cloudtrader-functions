using Moq;
using NUnit.Framework;
using System;
using Traders.Functions.ApiClients;

namespace Traders.Functions.Tests
{
    public class UpdateTraderBalanceByPrecipitationTests
    {
        [SetUp]
        public Tuple<Mock<ITradersApiClient>, UpdateTraderBalanceByPrecipitation> Setup()
        {
            var mockApiClient = new Mock<ITradersApiClient>();
            var mockFunction = new UpdateTraderBalanceByPrecipitation(mockApiClient.Object);

            return new Tuple<Mock<ITradersApiClient>, UpdateTraderBalanceByPrecipitation>(mockApiClient, mockFunction);
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}