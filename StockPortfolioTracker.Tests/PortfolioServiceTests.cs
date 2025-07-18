using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Models;
using StockPortfolioTracker.Services;
using Xunit;

namespace StockPortfolioTracker.Tests
{
    public class PortfolioServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IPortfolioService _portfolioService;

        public PortfolioServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _portfolioService = new PortfolioService(_context);
        }

        [Fact]
        public async Task CalculateCurrentHoldingsAsync_EmptyPortfolio_ReturnsEmpty()
        {
            // Act
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();

            // Assert
            Assert.Empty(holdings);
        }

        [Fact]
        public async Task CalculateCurrentHoldingsAsync_SingleBuyTrade_ReturnsCorrectHolding()
        {
            // Arrange
            var trade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow
            };
            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();

            // Act
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();

            // Assert
            var holdingsList = holdings.ToList();
            Assert.Single(holdingsList);
            
            var holding = holdingsList[0];
            Assert.Equal("AAPL", holding.StockSymbol);
            Assert.Equal(10, holding.Quantity);
            Assert.Equal(150.00m, holding.AverageCostBasis);
            Assert.Equal(1500.00m, holding.TotalValue);
        }

        [Fact]
        public async Task CalculateCurrentHoldingsAsync_MultipleBuyTrades_CalculatesCorrectAverageCost()
        {
            // Arrange
            var trade1 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 100.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var trade2 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 20,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(trade1, trade2);
            await _context.SaveChangesAsync();

            // Act
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();

            // Assert
            var holdingsList = holdings.ToList();
            Assert.Single(holdingsList);
            
            var holding = holdingsList[0];
            Assert.Equal("AAPL", holding.StockSymbol);
            Assert.Equal(30, holding.Quantity);
            // Average cost: (10 * 100 + 20 * 150) / 30 = 4000 / 30 = 133.33
            Assert.Equal(133.33m, Math.Round(holding.AverageCostBasis, 2));
        }

        [Fact]
        public async Task CalculateCurrentHoldingsAsync_BuyAndSellTrades_CalculatesCorrectQuantity()
        {
            // Arrange
            var buyTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 20,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var sellTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 5,
                Price = 160.00m,
                Type = TradeType.Sell,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(buyTrade, sellTrade);
            await _context.SaveChangesAsync();

            // Act
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();

            // Assert
            var holdingsList = holdings.ToList();
            Assert.Single(holdingsList);
            
            var holding = holdingsList[0];
            Assert.Equal("AAPL", holding.StockSymbol);
            Assert.Equal(15, holding.Quantity); // 20 - 5
            Assert.Equal(150.00m, holding.AverageCostBasis); // Cost basis remains same after sell
        }

        [Fact]
        public async Task CalculateCurrentHoldingsAsync_SellAllShares_ReturnsEmpty()
        {
            // Arrange
            var buyTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var sellTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 160.00m,
                Type = TradeType.Sell,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(buyTrade, sellTrade);
            await _context.SaveChangesAsync();

            // Act
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();

            // Assert
            Assert.Empty(holdings);
        }

        [Fact]
        public async Task CalculateHoldingsAsOfDateAsync_SpecificDate_ReturnsCorrectHoldings()
        {
            // Arrange
            var trade1 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var trade2 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 5,
                Price = 160.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1), // This should not be included
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(trade1, trade2);
            await _context.SaveChangesAsync();

            // Act - Get holdings as of 2 days ago (should only include trade1)
            var holdings = await _portfolioService.CalculateHoldingsAsOfDateAsync(DateTime.Today.AddDays(-2));

            // Assert
            var holdingsList = holdings.ToList();
            Assert.Single(holdingsList);
            
            var holding = holdingsList[0];
            Assert.Equal("AAPL", holding.StockSymbol);
            Assert.Equal(10, holding.Quantity); // Only first trade
            Assert.Equal(150.00m, holding.AverageCostBasis);
        }

        [Fact]
        public async Task GetCurrentQuantityAsync_ReturnsCorrectQuantity()
        {
            // Arrange
            var buyTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 15,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var sellTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 5,
                Price = 160.00m,
                Type = TradeType.Sell,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(buyTrade, sellTrade);
            await _context.SaveChangesAsync();

            // Act
            var quantity = await _portfolioService.GetCurrentQuantityAsync("AAPL");

            // Assert
            Assert.Equal(10, quantity); // 15 - 5
        }

        [Fact]
        public async Task GetQuantityAsOfDateAsync_ReturnsCorrectQuantityForDate()
        {
            // Arrange
            var trade1 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var trade2 = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 5,
                Price = 160.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(trade1, trade2);
            await _context.SaveChangesAsync();

            // Act - Get quantity as of 2 days ago (should only include trade1)
            var quantity = await _portfolioService.GetQuantityAsOfDateAsync("AAPL", DateTime.Today.AddDays(-2));

            // Assert
            Assert.Equal(10, quantity); // Only first trade
        }

        [Fact]
        public async Task GetQuantityAsOfDateAsync_NoTrades_ReturnsZero()
        {
            // Act
            var quantity = await _portfolioService.GetQuantityAsOfDateAsync("AAPL", DateTime.Today);

            // Assert
            Assert.Equal(0, quantity);
        }

        [Fact]
        public async Task CalculateCurrentHoldingsAsync_MultipleStocks_ReturnsAllHoldings()
        {
            // Arrange
            var appleTrade = new Trade
            {
                StockSymbol = "AAPL",
                Quantity = 10,
                Price = 150.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var msftTrade = new Trade
            {
                StockSymbol = "MSFT",
                Quantity = 5,
                Price = 300.00m,
                Type = TradeType.Buy,
                TradeDate = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.Trades.AddRange(appleTrade, msftTrade);
            await _context.SaveChangesAsync();

            // Act
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();

            // Assert
            var holdingsList = holdings.OrderBy(h => h.StockSymbol).ToList();
            Assert.Equal(2, holdingsList.Count);
            
            Assert.Equal("AAPL", holdingsList[0].StockSymbol);
            Assert.Equal(10, holdingsList[0].Quantity);
            
            Assert.Equal("MSFT", holdingsList[1].StockSymbol);
            Assert.Equal(5, holdingsList[1].Quantity);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}