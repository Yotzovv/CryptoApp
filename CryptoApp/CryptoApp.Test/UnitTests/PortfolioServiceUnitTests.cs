using Moq;
using CryptoApp.Data;
using CryptoApp.Data.Models;
using CryptoApp.Services.Implementations;
using CryptoApp.Data.Repository;
using CryptoApp.Data.dtos;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace CryptoApp.Tests
{
    [TestClass]
    public class PortfolioServiceTests
    {
        private Mock<CryptoAppDbContext> _mockContext = null!;
        private Mock<IRepository<Portfolio>> _mockPortfolioRepository = null!;
        private PortfolioService _portfolioService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CryptoAppDbContext>();
            _mockPortfolioRepository = new Mock<IRepository<Portfolio>>();
            _portfolioService = new PortfolioService(_mockContext.Object, _mockPortfolioRepository.Object);
        }

        [TestMethod]
        public async Task Get_ShouldReturnPortfolioDto()
        {
            var userId = Guid.NewGuid();
            var portfolio = new Portfolio
            {
                UserId = userId,
                CurrentPortfolioValue = 1000,
                InitialPortfolioValue = 800,
                Currencies = [],
                User = new AspNetUser()
            };
            _mockPortfolioRepository
                .Setup(x => x.FindAsync(
                    It.IsAny<Expression<Func<Portfolio, bool>>>(),
                    It.IsAny<Func<IQueryable<Portfolio>, IIncludableQueryable<Portfolio, object>>>()))
                .ReturnsAsync(portfolio);

            // Act
            var result = await _portfolioService.Get(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(portfolio.CurrentPortfolioValue, result.CurrentPortfolioValue);
            Assert.AreEqual(portfolio.InitialPortfolioValue, result.InitialPortfolioValue);
        }

        [TestMethod]
        public async Task Upload_ShouldAddNewCurrencyToPortfolio()
        {
            var user = new AspNetUser
            {
                Id = Guid.NewGuid(),
                Portfolio = new Portfolio
                {
                    UserId = Guid.NewGuid(),
                    Currencies = [],
                    User = null!
                }
            };
            var portfolio = user.Portfolio;

            _mockPortfolioRepository
                .Setup(x => x.FindAsync(
                    It.IsAny<Expression<Func<Portfolio, bool>>>(),
                    It.IsAny<Func<IQueryable<Portfolio>, IIncludableQueryable<Portfolio, object>>>()))
                .ReturnsAsync(portfolio);

            var fileMock = new Mock<IFormFile>();
            const string content = "10|BTC|50000";
            const string fileName = "test.txt";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(memoryStream.Length);

            await _portfolioService.Upload(fileMock.Object, user);

            Assert.IsTrue(portfolio.Currencies.Any(x => x.Coin == "BTC"));
            var addedCurrency = portfolio.Currencies.First(x => x.Coin == "BTC");
            Assert.AreEqual(10, addedCurrency.Amount);
            Assert.AreEqual(50000, addedCurrency.InitialBuyPrice);
        }

        [TestMethod]
        public async Task CalculateCurrentPortfolioValue_ShouldUpdatePortfolioValue()
        {
            var user = new AspNetUser
            {
                Id = Guid.NewGuid(),
                Portfolio = new Portfolio
                {
                    UserId = Guid.NewGuid(),
                    Currencies =
                    [
                        new Currency
                        {
                            Coin = "BTC",
                            Amount = 1,
                            InitialBuyPrice = 50000,
                            CurrentPrice = 55000,
                            Portfolio = null!
                        },

                        new Currency
                        {
                            Coin = "ETH",
                            Amount = 2,
                            InitialBuyPrice = 3000,
                            CurrentPrice = 3500,
                            Portfolio = null!
                        }
                    ],
                    User = null!
                }
            };
            var coinsInfoCache = new List<CoinloreItemDto>
            {
                new() { Symbol = "BTC", Price_Usd = 60000 },
                new() { Symbol = "ETH", Price_Usd = 4000 }
            };

            _mockPortfolioRepository
                .Setup(x => x.FindAsync(
                    It.IsAny<Expression<Func<Portfolio, bool>>>(),
                    It.IsAny<Func<IQueryable<Portfolio>, IIncludableQueryable<Portfolio, object>>>()))
                .ReturnsAsync(user.Portfolio);

            await _portfolioService.CalculateCurrentPortfolioValue(user, coinsInfoCache);

            var expectedTotalValue = (1 * 60000) + (2 * 4000);
            Assert.AreEqual(expectedTotalValue, user.Portfolio.CurrentPortfolioValue);
            _mockPortfolioRepository.Verify(x => x.UpdateAsync(user.Portfolio), Times.Once);
        }

        [TestMethod]
        public async Task CalculateInitialPortfolioValue_ShouldUpdatePortfolioInitialValue()
        {
            var user = new AspNetUser
            {
                Id = Guid.NewGuid(),
                Portfolio = new Portfolio
                {
                    UserId = Guid.NewGuid(),
                    Currencies =
                    [
                        new Currency
                        {
                            Coin = "BTC",
                            Amount = 1,
                            InitialBuyPrice = 50000,
                            CurrentPrice = 60000,
                            Portfolio = null!
                        },

                        new Currency
                        {
                            Coin = "ETH",
                            Amount = 2,
                            InitialBuyPrice = 3000,
                            CurrentPrice = 4000,
                            Portfolio = null!
                        }
                    ],
                    User = null!
                }
            };

            await _portfolioService.CalculateInitialPortfolioValue(user);

            const int expectedTotalValue = (1 * 50000) + (2 * 3000);
            Assert.AreEqual(expectedTotalValue, user.Portfolio.InitialPortfolioValue);
        }

        [TestMethod]
        public async Task UpdateOverallChangePercentage_ShouldUpdateChangePercentage()
        {
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                CurrentPortfolioValue = 120000,
                InitialPortfolioValue = 100000,
                User = null!
            };
            var user = new AspNetUser
            {
                Id = Guid.NewGuid(),
                Portfolio = portfolio
            };

            await _portfolioService.UpdateOverallChangePercentage(user);

            const double expectedChangePercentage = ((120000 - 100000) / 100000.0) * 100;
            Assert.AreEqual(expectedChangePercentage, portfolio.OverallChangePercentage);
            _mockPortfolioRepository.Verify(x => x.UpdateAsync(portfolio), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCoinsChangePercentages_ShouldUpdateCoinsChangePercentage()
        {
            var currency1 = new Currency
            {
                Coin = "BTC",
                InitialBuyPrice = 50000,
                CurrentPrice = 60000,
                Portfolio = null!
            };
            var currency2 = new Currency
            {
                Coin = "ETH",
                InitialBuyPrice = 3000,
                CurrentPrice = 4000,
                Portfolio = null!
            };
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                Currencies =
                [
                    currency1,
                    currency2
                ],
                User = null!
            };
            var user = new AspNetUser
            {
                Id = Guid.NewGuid(),
                Portfolio = portfolio
            };

            await _portfolioService.UpdateCoinsChangePercentages(user);

            const double expectedBtcChange = (60000 - 50000) / 50000.0 * 100;
            const double expectedEthChange = (4000 - 3000) / 3000.0 * 100;
    
            const double delta = 0.000001;

            Assert.AreEqual(expectedBtcChange, currency1.ChangePercentage, delta, "BTC change percentage is not within the acceptable delta.");
            Assert.AreEqual(expectedEthChange, currency2.ChangePercentage, delta, "ETH change percentage is not within the acceptable delta.");
    
            _mockPortfolioRepository.Verify(x => x.UpdateAsync(portfolio), Times.Once);
        }
    }
}
