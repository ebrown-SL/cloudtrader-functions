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

        private readonly Guid mockMineId = Guid.NewGuid();
        private readonly Guid mockTraderId = Guid.NewGuid();

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
        public async Task Run_WithEmptyMineId_ReturnsBadRequestObjectResult()
        {
            dynamic mockReqBody = new ExpandoObject();
            mockReqBody.MineId = new Guid();

            var mockReq = CreateMockRequest(mockReqBody);
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            var badReq = (BadRequestObjectResult)result;

            Assert.AreEqual("Request body was not in the expected format", badReq.Value);
        }

        [TestCase(0)]
        [TestCase(-20)]
        public async Task Run_WithInvalidPrecipitation_ReturnsOkObjectResult(int precipitation)
        {
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, precipitation);
            var mockReq = CreateMockRequest(mockReqBody);

            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            Assert.AreEqual($"Precipitation for mine {mockReqBody.MineId} was not greater than zero.", okResult.Value);
        }

        [Test]
        public async Task Run_WithNoTradersFound_ReturnsOkObjectResult()
        {
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, 20);
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

        [TestCase(20)]
        public async Task Run_TraderBalanceSuccessfullyUpdated_ReturnsOkObjectResult(int stock)
        {
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, 20);
            var mockReq = CreateMockRequest(mockReqBody);

            var mockTrader = new TraderCloudStockResponseModel(mockTraderId, mockMineId, stock);
            var mockGetTradersResp = new GetTradersByMineIdResponseModel() { Traders = new List<TraderCloudStockResponseModel>() { mockTrader } };

            var amountToAdd = mockReqBody.Precipitation * stock;

            var mockUpdatedTrader = new TraderResponseModel(mockTraderId, 4000);

            mockTradersApiClient
                .Setup(mock => mock.GetTraders(mockMineId))
                .ReturnsAsync(mockGetTradersResp);
            mockTradersApiClient
                .Setup(mock => mock.PatchTraderBalance(mockTraderId, amountToAdd))
                .ReturnsAsync(mockUpdatedTrader);
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            var resultList = new List<TraderResponseModel>() { mockUpdatedTrader };
            Assert.AreEqual($"Trader balances updated: {resultList.ToJson()}", okResult.Value);
        }

        [Test]
        public async Task Run_GetTradersFails_ReturnsStatusCodeResult500()
        {
            var mockReqBody = new TraderMineRevenueRequestModel(It.IsAny<Guid>(), It.IsAny<int>());
            var mockReq = CreateMockRequest(mockReqBody);

            mockTradersApiClient
                .Setup(mock => mock.GetTraders(It.IsAny<Guid>()))
                .Throws(new Exception());

            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            Assert.IsInstanceOf<StatusCodeResult>(result);

            var statusResult = (StatusCodeResult)result;

            Assert.AreEqual(500, statusResult.StatusCode);
        }
    }
}