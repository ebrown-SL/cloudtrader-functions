using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Traders.Functions.ApiClients;
using Microsoft.AspNetCore.Http;
using Traders.Functions.Models.Request;
using Traders.Functions.Helpers;
using System.IO;
using System.Text;
using System.Dynamic;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Traders.Functions.Models.Response;
using System.Collections.Generic;

namespace Traders.Functions.Tests
{
    public class UpdateTraderBalanceByPrecipitationTests
    {
        private Mock<ITradersApiClient> mockTradersApiClient;
        private Mock<ILogger> mockLogger;
        private UpdateTraderBalanceByPrecipitation functionUnderTest;

        private readonly Guid mockMineId = new Guid();

        [SetUp]
        public void SetupMockConfig()
        {
            mockTradersApiClient = new Mock<ITradersApiClient>();
            mockLogger = new Mock<ILogger>();
            functionUnderTest = new UpdateTraderBalanceByPrecipitation(mockTradersApiClient.Object);
        }

        private Mock<HttpRequest> CreateMockRequest(object body)
        {
            var json = body.ToJson();
            var byteArray = Encoding.ASCII.GetBytes(json);

            var memoryStream = new MemoryStream(byteArray);
            memoryStream.Flush();
            memoryStream.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(memoryStream);

            return mockRequest;
        }

        [Test]
        public void Run_WithNullMineId_ThrowsArgumentNullException()
        {
            dynamic mockReqBody = new ExpandoObject();
            mockReqBody.MineId = (Guid?)null;

            var mockReq = CreateMockRequest(mockReqBody);

            Assert.ThrowsAsync<ArgumentNullException>(() => functionUnderTest.Run(mockReq.Object, It.IsAny<ILogger>()));
        }

        [Test]
        public async Task Run_WithZeroPrecipitation_ReturnsOkObjectResult()
        {
            var mockReqBody = new TraderMineRevenueRequestModel() { MineId = mockMineId, Precipitation = 0 };
            var mockReq = CreateMockRequest(mockReqBody);

            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            Assert.AreEqual($"Precipitation for mine {mockReqBody.MineId} was 0.", okResult.Value);
        }

        [Test]
        public async Task Run_WithNoTradersFound_ReturnsOkObjectResult()
        {
            var mockReqBody = new TraderMineRevenueRequestModel() { MineId = mockMineId, Precipitation = 20 };
            var mockReq = CreateMockRequest(mockReqBody);
            var mockResp = new GetTradersByMineIdResponseModel();
            mockTradersApiClient
                .Setup(mock => mock.GetTraders(mockMineId))
                .ReturnsAsync(mockResp);
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            Assert.AreEqual($"No traders found with stock in mine id: {mockMineId}", okResult.Value);
        }

        [Test]
        public async Task Run_TradersSuccessfullyUpdated_ReturnsOkObjectResult()
        {
            var mockReqBody = new TraderMineRevenueRequestModel() { MineId = mockMineId, Precipitation = 20 };
            var mockReq = CreateMockRequest(mockReqBody);

            var mockTrader = new TraderCloudStockResponseModel() { Id = new Guid(), MineId = mockMineId, Stock = 20 };
            var mockResp = new GetTradersByMineIdResponseModel() { Traders = new List<TraderCloudStockResponseModel>() { mockTrader } };
            mockTradersApiClient
                .Setup(mock => mock.GetTraders(mockMineId))
                .ReturnsAsync(mockResp);
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            Assert.AreEqual($"Trader balances updated: {mockResp.Traders.ToJson()}", okResult.Value);
        }
    }
}