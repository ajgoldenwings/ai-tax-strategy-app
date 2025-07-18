using StockPortfolioTracker.Models;

namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Interface for trade operations and validation
    /// </summary>
    public interface ITradeService
    {
        /// <summary>
        /// Adds a new trade with validation
        /// </summary>
        /// <param name="trade">The trade to add</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> AddTradeAsync(Trade trade);

        /// <summary>
        /// Gets all trades in chronological order
        /// </summary>
        /// <returns>List of all trades ordered by trade date</returns>
        Task<IEnumerable<Trade>> GetAllTradesAsync();

        /// <summary>
        /// Gets trades for a specific stock symbol
        /// </summary>
        /// <param name="stockSymbol">The stock symbol to filter by</param>
        /// <returns>List of trades for the specified stock</returns>
        Task<IEnumerable<Trade>> GetTradesBySymbolAsync(string stockSymbol);

        /// <summary>
        /// Validates a trade according to business rules
        /// </summary>
        /// <param name="trade">The trade to validate</param>
        /// <returns>Validation result with any error messages</returns>
        Task<ValidationResult> ValidateTradeAsync(Trade trade);

        /// <summary>
        /// Gets trades up to a specific date
        /// </summary>
        /// <param name="date">The cutoff date</param>
        /// <returns>List of trades up to the specified date</returns>
        Task<IEnumerable<Trade>> GetTradesUpToDateAsync(DateTime date);

        /// <summary>
        /// Deletes a trade by ID
        /// </summary>
        /// <param name="tradeId">The ID of the trade to delete</param>
        /// <returns>True if the trade was deleted successfully, false otherwise</returns>
        Task<bool> DeleteTradeAsync(int tradeId);
    }
}