using StockPortfolioTracker.Models;

namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Interface for portfolio calculations and operations
    /// </summary>
    public interface IPortfolioService
    {
        /// <summary>
        /// Calculates current portfolio holdings
        /// </summary>
        /// <returns>List of current portfolio holdings</returns>
        Task<IEnumerable<PortfolioHolding>> CalculateCurrentHoldingsAsync();

        /// <summary>
        /// Calculates portfolio holdings as of a specific date
        /// </summary>
        /// <param name="date">The date to calculate holdings for</param>
        /// <returns>List of portfolio holdings as of the specified date</returns>
        Task<IEnumerable<PortfolioHolding>> CalculateHoldingsAsOfDateAsync(DateTime date);

        /// <summary>
        /// Gets the current quantity held for a specific stock
        /// </summary>
        /// <param name="stockSymbol">The stock symbol</param>
        /// <returns>Current quantity held</returns>
        Task<decimal> GetCurrentQuantityAsync(string stockSymbol);

        /// <summary>
        /// Gets the quantity held for a specific stock as of a date
        /// </summary>
        /// <param name="stockSymbol">The stock symbol</param>
        /// <param name="date">The date to check</param>
        /// <returns>Quantity held as of the specified date</returns>
        Task<decimal> GetQuantityAsOfDateAsync(string stockSymbol, DateTime date);
    }

    /// <summary>
    /// Represents a portfolio holding for a specific stock
    /// </summary>
    public class PortfolioHolding
    {
        /// <summary>
        /// Stock symbol
        /// </summary>
        public string StockSymbol { get; set; } = string.Empty;

        /// <summary>
        /// Current quantity held
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Average cost basis per share
        /// </summary>
        public decimal AverageCostBasis { get; set; }

        /// <summary>
        /// Total value of the holding (Quantity * AverageCostBasis)
        /// </summary>
        public decimal TotalValue => Quantity * AverageCostBasis;
    }
}