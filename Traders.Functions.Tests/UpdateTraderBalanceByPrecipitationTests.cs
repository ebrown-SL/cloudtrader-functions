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
            // Arrange
            dynamic mockReqBody = new ExpandoObject();
            mockReqBody.MineId = new Guid();

            var mockReq = CreateMockRequest(mockReqBody);

            // Act
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badReq = (BadRequestObjectResult)result;
            Assert.AreEqual("Request body was not in the expected format", badReq.Value);
            mockTradersApiClient.Verify(mock => mock.GetTraders(It.IsAny<Guid>()), Times.Never);
            mockTradersApiClient.Verify(mock => mock.PatchTraderBalance(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }

        [TestCase(0)]
        [TestCase(-20)]
        public async Task Run_WithInvalidPrecipitation_ReturnsOkObjectResult(int precipitation)
        {
            // Arrange
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, precipitation);
            var mockReq = CreateMockRequest(mockReqBody);

            // Act
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual($"Precipitation for mine {mockReqBody.MineId} was not greater than zero.", okResult.Value);
            mockTradersApiClient.Verify(mock => mock.GetTraders(It.IsAny<Guid>()), Times.Never);
            mockTradersApiClient.Verify(mock => mock.PatchTraderBalance(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task Run_WithNoTradersFound_ReturnsOkObjectResult()
        {
            // Arrange
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, 20);
            var mockReq = CreateMockRequest(mockReqBody);
            var mockResp = new GetTradersByMineIdResponseModel();
            mockTradersApiClient
                .Setup(mock => mock.GetTraders(mockMineId))
                .ReturnsAsync(mockResp);

            // Act
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual($"No traders found with stock in mine id: {mockMineId}", okResult.Value);
            mockTradersApiClient.Verify(mock => mock.GetTraders(It.IsAny<Guid>()), Times.Once);
            mockTradersApiClient.Verify(mock => mock.PatchTraderBalance(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }

        [TestCase(20)]
        public async Task Run_TraderBalanceSuccessfullyUpdated_ReturnsOkObjectResult(int stock)
        {
            // Arrange
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, 20);
            var mockReq = CreateMockRequest(mockReqBody);

            var mockTrader = new TraderCloudStockResponseModel(mockTraderId, mockMineId, stock);

            var mockGetTradersResp = new GetTradersByMineIdResponseModel() { Traders = new List<TraderCloudStockResponseModel>() { mockTrader, mockTrader, mockTrader } };

            var amountToAdd = mockReqBody.Precipitation * stock;

            var mockUpdatedTrader = new TraderResponseModel(mockTraderId, 4000);
            var mockUpdatedTraders = new List<TraderResponseModel>() { mockUpdatedTrader, mockUpdatedTrader, mockUpdatedTrader };

            mockTradersApiClient
                .Setup(mock => mock.GetTraders(mockMineId))
                .ReturnsAsync(mockGetTradersResp);
            mockTradersApiClient
                .Setup(mock => mock.PatchTraderBalance(mockTraderId, amountToAdd))
                .ReturnsAsync(mockUpdatedTrader);

            // Act
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual($"Trader balances updated: {mockUpdatedTraders.ToJson()}", okResult.Value);
            mockTradersApiClient.Verify(mock => mock.GetTraders(It.IsAny<Guid>()), Times.Once);
            mockTradersApiClient.Verify(mock => mock.PatchTraderBalance(It.IsAny<Guid>(), It.IsAny<int>()), Times.Exactly(3));
        }

        [Test]
        public async Task Run_GetTradersFromTradersServiceFails_ReturnsStatusCodeResult500()
        {
            // Arrange
            var mockReqBody = new TraderMineRevenueRequestModel(mockMineId, 20);
            var mockReq = CreateMockRequest(mockReqBody);

            mockTradersApiClient
                .Setup(mock => mock.GetTraders(It.Is<Guid>(id => id.Equals(mockMineId))))
                .ThrowsAsync(new Exception());

            // Act
            var result = await functionUnderTest.Run(mockReq.Object, mockLogger.Object);

            // Assert
            Assert.IsInstanceOf<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.AreEqual(500, statusCodeResult.StatusCode);
            mockTradersApiClient.Verify(mock => mock.GetTraders(It.IsAny<Guid>()), Times.Once);
            mockTradersApiClient.Verify(mock => mock.PatchTraderBalance(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }
    }
}