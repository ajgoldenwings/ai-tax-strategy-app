using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Models;
using StockPortfolioTracker.Services;
using Xunit;

namespace StockPortfolioTracker.Tests
{
    public class TradeServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IPortfolioService _portfolioService;
        private readonly ITradeService _tradeService;

        public TradeServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _portfolioService = new PortfolioService(_context);
            _tradeService = new TradeService(_context, _portfolioService);
        }

        [Fact]
        public async Task ValidateTradeAsync_ValidBuyTrade_ReturnsValid()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(trade);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.ErrorMessages);
        }

        [Fact]
        public async Task ValidateTradeAsync_EmptyStockSymbol_ReturnsInvalid()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(trade);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Stock symbol is required", result.ErrorMessages);
        }

        [Fact]
        public async Task ValidateTradeAsync_InvalidStockSymbol_ReturnsInvalid()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL@123",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(trade);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Stock symbol must contain only alphanumeric characters", result.ErrorMessages);
        }

        [Fact]
        public async Task ValidateTradeAsync_NegativeQuantity_ReturnsInvalid()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = -10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(trade);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Quantity must be greater than 0", result.ErrorMessages);
        }

        [Fact]
        public async Task ValidateTradeAsync_NegativePrice_ReturnsInvalid()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = -150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(trade);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Price must be greater than 0", result.ErrorMessages);
        }

        [Fact]
        public async Task ValidateTradeAsync_FutureTradeDate_ReturnsInvalid()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(trade);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Trade date cannot be in the future", result.ErrorMessages);
        }

        [Fact]
        public async Task ValidateTradeAsync_SellMoreThanOwned_ReturnsInvalid()
        {
            // Arrange
            // First add a buy trade
            var buyTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 5,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-2)
            };
            await _tradeService.AddTradeAsync(buyTrade);

            // Try to sell more than owned
            var sellTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 160.00m,
                Type = TradeType.Sell,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.ValidateTradeAsync(sellTrade);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Cannot sell 10 shares. Only 5 shares available", result.ErrorMessages);
        }

        [Fact]
        public async Task AddTradeAsync_ValidTrade_ReturnsTrue()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _tradeService.AddTradeAsync(trade);

            // Assert
            Assert.True(result);
            var savedTrade = await _context.Trades.FirstOrDefaultAsync();
            Assert.NotNull(savedTrade);
            Assert.Equal("AAPL", savedTrade.StockSymbol);
        }

        [Fact]
        public async Task GetAllTradesAsync_ReturnsTradesInChronologicalOrder()
        {
            // Arrange
            var trade1 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-2)
            };

            var trade2 = new Trade
            {
                StockSymbol = "MSFT",
                Quantity = 5,
                Price = 300.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            await _tradeService.AddTradeAsync(trade2); // Add in reverse order
            await _tradeService.AddTradeAsync(trade1);

            // Act
            var trades = await _tradeService.GetAllTradesAsync();

            // Assert
            var tradeList = trades.ToList();
            Assert.Equal(2, tradeList.Count);
            Assert.Equal("AAPL", tradeList[0].StockSymbol); // Earlier date should be first
            Assert.Equal("MSFT", tradeList[1].StockSymbol);
        }

        [Fact]
        public async Task GetTradesBySymbolAsync_ReturnsOnlyMatchingSymbol()
        {
            // Arrange
            var appleTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            var msftTrade = new Trade
            {
                StockSymbol = "MSFT",
                Quantity = 5,
                Price = 300.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1)
            };

            await _tradeService.AddTradeAsync(appleTrade);
            await _tradeService.AddTradeAsync(msftTrade);

            // Act
            var appleTrades = await _tradeService.GetTradesBySymbolAsync("AAPL");

            // Assert
            var tradeList = appleTrades.ToList();
            Assert.Single(tradeList);
            Assert.Equal("AAPL", tradeList[0].StockSymbol);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}