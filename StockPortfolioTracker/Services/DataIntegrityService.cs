using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Models;

namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Service for checking and maintaining data integrity
    /// </summary>
    public class DataIntegrityService : IDataIntegrityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataIntegrityService> _logger;

        public DataIntegrityService(ApplicationDbContext context, ILogger<DataIntegrityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Performs comprehensive data integrity check
        /// </summary>
        public async Task<DataIntegrityResult> CheckDataIntegrityAsync()
        {
            var result = new DataIntegrityResult
            {
                LastChecked = DateTime.UtcNow,
                IsHealthy = true
            };

            try
            {
                // Check database connection
                if (!await ValidateDatabaseConnectionAsync())
                {
                    result.Issues.Add("Database connection failed");
                    result.IsHealthy = false;
                    return result;
                }

                // Get total trade count
                result.TotalTrades = await _context.Trades.CountAsync();

                // Check for corrupted data
                var corruptionIssues = await CheckForCorruptedDataAsync();
                result.Issues.AddRange(corruptionIssues);
                result.CorruptedTrades = corruptionIssues.Count();

                // Check for data consistency issues
                await CheckDataConsistencyAsync(result);

                // Check for performance issues
                await CheckPerformanceIssuesAsync(result);

                result.IsHealthy = !result.Issues.Any();

                _logger.LogInformation("Data integrity check completed. Healthy: {IsHealthy}, Issues: {IssueCount}, Warnings: {WarningCount}",
                    result.IsHealthy, result.Issues.Count, result.Warnings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data integrity check");
                result.Issues.Add($"Data integrity check failed: {ex.Message}");
                result.IsHealthy = false;
            }

            return result;
        }

        /// <summary>
        /// Validates database connection and basic operations
        /// </summary>
        public async Task<bool> ValidateDatabaseConnectionAsync()
        {
            try
            {
                // Try to execute a simple query
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection validation failed");
                return false;
            }
        }

        /// <summary>
        /// Checks for corrupted trade data
        /// </summary>
        public async Task<IEnumerable<string>> CheckForCorruptedDataAsync()
        {
            var issues = new List<string>();

            try
            {
                // Check for trades with invalid quantities
                var invalidQuantityCount = await _context.Trades
                    .Where(t => t.Quantity <= 0)
                    .CountAsync();

                if (invalidQuantityCount > 0)
                {
                    issues.Add($"Found {invalidQuantityCount} trades with invalid quantities (≤ 0)");
                }

                // Check for trades with invalid prices
                var invalidPriceCount = await _context.Trades
                    .Where(t => t.Price <= 0)
                    .CountAsync();

                if (invalidPriceCount > 0)
                {
                    issues.Add($"Found {invalidPriceCount} trades with invalid prices (≤ 0)");
                }

                // Check for trades with empty stock symbols
                var emptySymbolCount = await _context.Trades
                    .Where(t => string.IsNullOrWhiteSpace(t.StockSymbol))
                    .CountAsync();

                if (emptySymbolCount > 0)
                {
                    issues.Add($"Found {emptySymbolCount} trades with empty stock symbols");
                }

                // Check for trades with future dates
                var futureDateCount = await _context.Trades
                    .Where(t => t.TradeDate > DateTime.Today)
                    .CountAsync();

                if (futureDateCount > 0)
                {
                    issues.Add($"Found {futureDateCount} trades with future dates");
                }

                // Check for trades with invalid stock symbols (too long or invalid characters)
                var invalidSymbolCount = await _context.Trades
                    .Where(t => t.StockSymbol.Length > 10)
                    .CountAsync();

                if (invalidSymbolCount > 0)
                {
                    issues.Add($"Found {invalidSymbolCount} trades with stock symbols longer than 10 characters");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for corrupted data");
                issues.Add($"Error checking data corruption: {ex.Message}");
            }

            return issues;
        }

        /// <summary>
        /// Attempts to repair minor data corruption issues
        /// </summary>
        public async Task<bool> RepairDataCorruptionAsync()
        {
            try
            {
                var repairCount = 0;

                // Fix stock symbols that are too long (truncate to 10 characters)
                var longSymbolTrades = await _context.Trades
                    .Where(t => t.StockSymbol.Length > 10)
                    .ToListAsync();

                foreach (var trade in longSymbolTrades)
                {
                    trade.StockSymbol = trade.StockSymbol.Substring(0, 10);
                    repairCount++;
                }

                // Fix future trade dates (set to today)
                var futureTrades = await _context.Trades
                    .Where(t => t.TradeDate > DateTime.Today)
                    .ToListAsync();

                foreach (var trade in futureTrades)
                {
                    trade.TradeDate = DateTime.Today;
                    repairCount++;
                }

                if (repairCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Repaired {RepairCount} data corruption issues", repairCount);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repairing data corruption");
                return false;
            }
        }

        /// <summary>
        /// Checks for data consistency issues
        /// </summary>
        private async Task CheckDataConsistencyAsync(DataIntegrityResult result)
        {
            try
            {
                // Check for orphaned data or inconsistencies
                var duplicateTradesCount = await _context.Trades
                    .GroupBy(t => new { t.StockSymbol, t.TradeDate, t.Quantity, t.Price, t.Type })
                    .Where(g => g.Count() > 1)
                    .CountAsync();

                if (duplicateTradesCount > 0)
                {
                    result.Warnings.Add($"Found {duplicateTradesCount} potential duplicate trade groups");
                }

                // Check for unusual trade patterns
                var veryOldTrades = await _context.Trades
                    .Where(t => t.TradeDate < DateTime.Today.AddYears(-10))
                    .CountAsync();

                if (veryOldTrades > 0)
                {
                    result.Warnings.Add($"Found {veryOldTrades} trades older than 10 years");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking data consistency");
                result.Issues.Add($"Data consistency check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks for performance-related issues
        /// </summary>
        private async Task CheckPerformanceIssuesAsync(DataIntegrityResult result)
        {
            try
            {
                var totalTrades = await _context.Trades.CountAsync();

                if (totalTrades > 10000)
                {
                    result.Warnings.Add($"Large number of trades ({totalTrades:N0}) may impact performance");
                }

                // Check for missing indexes (this would be more complex in a real scenario)
                if (totalTrades > 1000)
                {
                    result.Warnings.Add("Consider adding database indexes for better performance with large datasets");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking performance issues");
                result.Warnings.Add($"Performance check failed: {ex.Message}");
            }
        }
    }
}