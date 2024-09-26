using CryptoApp.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace CryptoApp.Tests.Services
{
    [TestClass]
    public class CoinloreServiceTests
    {
        private CoinloreService _coinloreService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(config => config["CoinloreURL"]).Returns("https://api.coinlore.net/api");

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.coinlore.net")
            };

            _coinloreService = new CoinloreService(_mockConfiguration.Object, _httpClient);

            CoinloreService.CoinInfoCache.Clear();
        }

        [TestMethod]
        public async Task FetchCoinsInBatchAsync_ShouldReturnCoinloreItems_WhenApiCallIsSuccessful()
        {
            var responseContent = @"{
        ""data"": [
            { ""id"": 1, ""symbol"": ""BTC"" },
            { ""id"": 2, ""symbol"": ""ETH"" }
        ]
    }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var result = await _coinloreService.FetchCoinsInBatchAsync(0, 2);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("BTC", result[0].Symbol);
            Assert.AreEqual("ETH", result[1].Symbol);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "An error occurred while fetching coins from Coinlore API. Please try again later.")]
        public async Task FetchCoinsInBatchAsync_ShouldThrowException_WhenHttpRequestFails()
        {
            // Arrange: Mock a failed response (HTTP error)
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act and Assert: Ensure the exception is thrown
            await _coinloreService.FetchCoinsInBatchAsync(0, 2);
        }

        [TestMethod]
        public async Task FetchCoinsInBatchAsync_ShouldPopulateCoinInfoCache_WhenApiCallIsSuccessful()
        {
            var responseContent = @"{
        ""data"": [
            { ""id"": 1, ""symbol"": ""BTC"" },
            { ""id"": 2, ""symbol"": ""ETH"" }
        ]
    }";
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            await _coinloreService.FetchCoinsInBatchAsync(0, 2);

            Assert.IsTrue(CoinloreService.CoinInfoCache.ContainsKey("BTC"));
            Assert.IsTrue(CoinloreService.CoinInfoCache.ContainsKey("ETH"));
        }
    }
}
