using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Models;

namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Service for seeding the database with sample data for development and demonstration
    /// </summary>
    public class SeedDataService : ISeedDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ApplicationDbContext context, ILogger<SeedDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the database with sample trade data for demonstration purposes
        /// </summary>
        public async Task SeedDataAsync()
        {
            try
            {
                // Check if seed data already exists
                if (await HasSeedDataAsync())
                {
                    _logger.LogInformation("Seed data already exists. Skipping seed operation.");
                    return;
                }

                _logger.LogInformation("Starting seed data creation...");

                var seedTrades = CreateSeedTrades();
                
                await _context.Trades.AddRangeAsync(seedTrades);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {TradeCount} trades into the database.", seedTrades.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding data.");
                throw;
            }
        }

        /// <summary>
        /// Checks if seed data already exists in the database
        /// </summary>
        public async Task<bool> HasSeedDataAsync()
        {
            return await _context.Trades.AnyAsync();
        }

        /// <summary>
        /// Clears all seed data from the database
        /// </summary>
        public async Task ClearSeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Clearing all seed data...");
                
                var allTrades = await _context.Trades.ToListAsync();
                _context.Trades.RemoveRange(allTrades);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully cleared {TradeCount} trades from the database.", allTrades.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while clearing seed data.");
                throw;
            }
        }

        /// <summary>
        /// Creates a comprehensive set of seed trades for demonstration
        /// </summary>
        private List<Trade> CreateSeedTrades()
        {
            var trades = new List<Trade>();
            var baseDate = DateTime.Now.AddMonths(-6); // Start 6 months ago

            // Create a realistic trading scenario with multiple stocks
            // This will demonstrate portfolio evolution over time

            // Initial purchases - Building positions
            trades.AddRange(CreateInitialPurchases(baseDate));
            
            // Regular trading activity over time
            trades.AddRange(CreateRegularTradingActivity(baseDate.AddDays(30)));
            
            // Some profit-taking and rebalancing
            trades.AddRange(CreateRebalancingTrades(baseDate.AddDays(90)));
            
            // Recent activity
            trades.AddRange(CreateRecentTrades(baseDate.AddDays(150)));

            return trades.OrderBy(t => t.TradeDate).ToList();
        }

        /// <summary>
        /// Creates initial purchase trades to establish positions
        /// </summary>
        private List<Trade> CreateInitialPurchases(DateTime startDate)
        {
            return new List<Trade>
            {
                new Trade
                {
                    StockSymbol = "AAPL",
                    Quantity = 50,
                    Price = 150.25m,
                    Type = TradeType.Buy,
                    TradeDate = startDate,
                    CreatedAt = DateTime.UtcNow
                },
                new Trade
                {
                    StockSymbol = "MSFT",
                    Quantity = 30,
                    Price = 280.50m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(2),
                    CreatedAt = DateTime.UtcNow
                },
                new Trade
                {
                    StockSymbol = "GOOGL",
                    Quantity = 15,
                    Price = 2650.75m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(5),
                    CreatedAt = DateTime.UtcNow
                },
                new Trade
                {
                    StockSymbol = "TSLA",
                    Quantity = 25,
                    Price = 220.30m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                },
                new Trade
                {
                    StockSymbol = "NVDA",
                    Quantity = 20,
                    Price = 450.80m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(10),
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Creates regular trading activity to show ongoing portfolio management
        /// </summary>
        private List<Trade> CreateRegularTradingActivity(DateTime startDate)
        {
            return new List<Trade>
            {
                // Adding to existing positions
                new Trade
                {
                    StockSymbol = "AAPL",
                    Quantity = 25,
                    Price = 155.75m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(15),
                    CreatedAt = DateTime.UtcNow
                },
                new Trade
                {
                    StockSymbol = "MSFT",
                    Quantity = 20,
                    Price = 285.20m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(20),
                    CreatedAt = DateTime.UtcNow
                },
                // New position
                new Trade
                {
                    StockSymbol = "AMZN",
                    Quantity = 12,
                    Price = 3200.45m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(25),
                    CreatedAt = DateTime.UtcNow
                },
                // Adding to NVDA position
                new Trade
                {
                    StockSymbol = "NVDA",
                    Quantity = 15,
                    Price = 475.60m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Creates rebalancing trades showing profit-taking and position adjustments
        /// </summary>
        private List<Trade> CreateRebalancingTrades(DateTime startDate)
        {
            return new List<Trade>
            {
                // Profit-taking on TSLA
                new Trade
                {
                    StockSymbol = "TSLA",
                    Quantity = 10,
                    Price = 245.80m,
                    Type = TradeType.Sell,
                    TradeDate = startDate.AddDays(10),
                    CreatedAt = DateTime.UtcNow
                },
                // Partial profit on NVDA
                new Trade
                {
                    StockSymbol = "NVDA",
                    Quantity = 8,
                    Price = 520.25m,
                    Type = TradeType.Sell,
                    TradeDate = startDate.AddDays(15),
                    CreatedAt = DateTime.UtcNow
                },
                // Adding new tech stock
                new Trade
                {
                    StockSymbol = "META",
                    Quantity = 18,
                    Price = 320.90m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(20),
                    CreatedAt = DateTime.UtcNow
                },
                // Diversifying into different sector
                new Trade
                {
                    StockSymbol = "JPM",
                    Quantity = 35,
                    Price = 145.60m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(25),
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Creates recent trading activity to show current portfolio state
        /// </summary>
        private List<Trade> CreateRecentTrades(DateTime startDate)
        {
            return new List<Trade>
            {
                // Recent additions
                new Trade
                {
                    StockSymbol = "AAPL",
                    Quantity = 10,
                    Price = 175.30m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(5),
                    CreatedAt = DateTime.UtcNow
                },
                new Trade
                {
                    StockSymbol = "GOOGL",
                    Quantity = 5,
                    Price = 2750.20m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(10),
                    CreatedAt = DateTime.UtcNow
                },
                // Small position in emerging stock
                new Trade
                {
                    StockSymbol = "AMD",
                    Quantity = 40,
                    Price = 95.75m,
                    Type = TradeType.Buy,
                    TradeDate = startDate.AddDays(15),
                    CreatedAt = DateTime.UtcNow
                },
                // Recent partial sale
                new Trade
                {
                    StockSymbol = "AMZN",
                    Quantity = 3,
                    Price = 3350.80m,
                    Type = TradeType.Sell,
                    TradeDate = startDate.AddDays(20),
                    CreatedAt = DateTime.UtcNow
                },
                // Very recent trade
                new Trade
                {
                    StockSymbol = "MSFT",
                    Quantity = 15,
                    Price = 295.45m,
                    Type = TradeType.Buy,
                    TradeDate = DateTime.Now.AddDays(-3),
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}