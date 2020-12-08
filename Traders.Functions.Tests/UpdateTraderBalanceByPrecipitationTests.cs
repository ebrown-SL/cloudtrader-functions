using Moq;
using NUnit.Framework;
using Traders.Functions;

namespace Traders.Functions.Tests
{
    public class UpdateTraderBalanceByPrecipitationTests
    {
        [SetUp]
        public void Setup()
        {
            var mockApiClient = new Mock<ITradersApiClient>();
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}