using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Models;
using System.Text.RegularExpressions;

namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Service for trade operations and validation
    /// </summary>
    public class TradeService : ITradeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPortfolioService _portfolioService;

        public TradeService(ApplicationDbContext context, IPortfolioService portfolioService)
        {
            _context = context;
            _portfolioService = portfolioService;
        }

        /// <summary>
        /// Adds a new trade with validation and automatic retry on transient failures
        /// </summary>
        public async Task<bool> AddTradeAsync(Trade trade)
        {
            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var validationResult = await ValidateTradeAsync(trade);
                    if (!validationResult.IsValid)
                    {
                        return false;
                    }

                    trade.CreatedAt = DateTime.UtcNow;
                    _context.Trades.Add(trade);
                    
                    // Ensure data is saved with automatic retry on transient failures
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException ex) when (IsTransientFailure(ex) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount)); // Exponential backoff
                    
                    // Reset the context state for retry
                    _context.Entry(trade).State = EntityState.Detached;
                }
                catch (Exception)
                {
                    // Log the exception if needed, but don't expose details to the user
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all trades in chronological order
        /// </summary>
        public async Task<IEnumerable<Trade>> GetAllTradesAsync()
        {
            try
            {
                return await _context.Trades
                    .OrderBy(t => t.TradeDate)
                    .ThenBy(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Return empty list on error to prevent application crash
                return new List<Trade>();
            }
        }

        /// <summary>
        /// Gets trades for a specific stock symbol
        /// </summary>
        public async Task<IEnumerable<Trade>> GetTradesBySymbolAsync(string stockSymbol)
        {
            try
            {
                return await _context.Trades
                    .Where(t => t.StockSymbol.ToUpper() == stockSymbol.ToUpper())
                    .OrderBy(t => t.TradeDate)
                    .ThenBy(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Return empty list on error to prevent application crash
                return new List<Trade>();
            }
        }

        /// <summary>
        /// Gets trades up to a specific date
        /// </summary>
        public async Task<IEnumerable<Trade>> GetTradesUpToDateAsync(DateTime date)
        {
            try
            {
                return await _context.Trades
                    .Where(t => t.TradeDate <= date)
                    .OrderBy(t => t.TradeDate)
                    .ThenBy(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Return empty list on error to prevent application crash
                return new List<Trade>();
            }
        }

        /// <summary>
        /// Validates a trade according to business rules
        /// </summary>
        public async Task<ValidationResult> ValidateTradeAsync(Trade trade)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate stock symbol
            if (string.IsNullOrWhiteSpace(trade.StockSymbol))
            {
                result.ErrorMessages.Add("Stock symbol is required");
                result.IsValid = false;
            }
            else if (trade.StockSymbol.Length > 10)
            {
                result.ErrorMessages.Add("Stock symbol must be 10 characters or less");
                result.IsValid = false;
            }
            else if (!Regex.IsMatch(trade.StockSymbol, @"^[A-Za-z0-9]+$"))
            {
                result.ErrorMessages.Add("Stock symbol must contain only alphanumeric characters");
                result.IsValid = false;
            }

            // Validate quantity
            if (trade.Quantity <= 0)
            {
                result.ErrorMessages.Add("Quantity must be greater than 0");
                result.IsValid = false;
            }

            // Validate price
            if (trade.Price <= 0)
            {
                result.ErrorMessages.Add("Price must be greater than 0");
                result.IsValid = false;
            }

            // Validate trade date
            if (trade.TradeDate > DateTime.Today)
            {
                result.ErrorMessages.Add("Trade date cannot be in the future");
                result.IsValid = false;
            }

            // For sell trades, validate that we have enough shares
            if (trade.Type == TradeType.Sell)
            {
                try
                {
                    // Get current holdings as of the trade date (excluding the current trade if it's an update)
                    var currentQuantity = await _portfolioService.GetQuantityAsOfDateAsync(trade.StockSymbol, trade.TradeDate);
                    
                    // If this is an existing trade being updated, we need to add back its quantity
                    if (trade.Id > 0)
                    {
                        var existingTrade = await _context.Trades.FindAsync(trade.Id);
                        if (existingTrade != null && existingTrade.Type == TradeType.Buy)
                        {
                            currentQuantity += existingTrade.Quantity;
                        }
                        else if (existingTrade != null && existingTrade.Type == TradeType.Sell)
                        {
                            currentQuantity -= existingTrade.Quantity;
                        }
                    }
                    
                    if (trade.Quantity > currentQuantity)
                    {
                        result.ErrorMessages.Add($"Cannot sell {trade.Quantity:N4} shares of {trade.StockSymbol}. Only {currentQuantity:N4} shares available as of {trade.TradeDate:yyyy-MM-dd}");
                        result.IsValid = false;
                    }
                }
                catch (Exception)
                {
                    // If we can't validate holdings, allow the trade but log a warning
                    result.WarningMessages.Add("Unable to verify current holdings for sell validation");
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes a trade by ID with automatic data saving
        /// </summary>
        public async Task<bool> DeleteTradeAsync(int tradeId)
        {
            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var trade = await _context.Trades.FindAsync(tradeId);
                    if (trade == null)
                    {
                        return false;
                    }

                    _context.Trades.Remove(trade);
                    
                    // Ensure data is saved with automatic retry on transient failures
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException ex) when (IsTransientFailure(ex) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount)); // Exponential backoff
                }
                catch (Exception)
                {
                    // Log the exception if needed, but don't expose details to the user
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a database exception is transient and should be retried
        /// </summary>
        private static bool IsTransientFailure(Exception exception)
        {
            // Check for common transient database errors
            var message = exception.Message.ToLowerInvariant();
            
            return message.Contains("timeout") ||
                   message.Contains("deadlock") ||
                   message.Contains("connection") ||
                   message.Contains("network") ||
                   message.Contains("transport") ||
                   exception is TimeoutException ||
                   (exception.InnerException != null && IsTransientFailure(exception.InnerException));
        }
    }
}