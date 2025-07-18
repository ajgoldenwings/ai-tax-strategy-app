using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Models;

namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Service for portfolio calculations and operations
    /// </summary>
    public class PortfolioService : IPortfolioService
    {
        private readonly ApplicationDbContext _context;

        public PortfolioService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates current portfolio holdings
        /// </summary>
        public async Task<IEnumerable<PortfolioHolding>> CalculateCurrentHoldingsAsync()
        {
            try
            {
                return await CalculateHoldingsAsOfDateAsync(DateTime.Today);
            }
            catch (Exception)
            {
                // Return empty list on error to prevent application crash
                return new List<PortfolioHolding>();
            }
        }

        /// <summary>
        /// Calculates portfolio holdings as of a specific date
        /// </summary>
        public async Task<IEnumerable<PortfolioHolding>> CalculateHoldingsAsOfDateAsync(DateTime date)
        {
            try
            {
                var trades = await _context.Trades
                    .Where(t => t.TradeDate <= date)
                    .OrderBy(t => t.TradeDate)
                    .ThenBy(t => t.CreatedAt)
                    .ToListAsync();

                var holdings = new Dictionary<string, PortfolioHolding>();

                foreach (var trade in trades)
                {
                    var symbol = trade.StockSymbol.ToUpper();
                    
                    if (!holdings.ContainsKey(symbol))
                    {
                        holdings[symbol] = new PortfolioHolding
                        {
                            StockSymbol = symbol,
                            Quantity = 0,
                            AverageCostBasis = 0
                        };
                    }

                    var holding = holdings[symbol];

                    if (trade.Type == TradeType.Buy)
                    {
                        // Calculate new average cost basis using weighted average
                        var totalValue = (holding.Quantity * holding.AverageCostBasis) + (trade.Quantity * trade.Price);
                        var totalQuantity = holding.Quantity + trade.Quantity;
                        
                        holding.AverageCostBasis = totalQuantity > 0 ? totalValue / totalQuantity : 0;
                        holding.Quantity = totalQuantity;
                    }
                    else // Sell
                    {
                        holding.Quantity -= trade.Quantity;
                        
                        // If all shares are sold, reset cost basis
                        if (holding.Quantity <= 0)
                        {
                            holding.Quantity = 0;
                            holding.AverageCostBasis = 0;
                        }
                    }
                }

                // Return only holdings with positive quantities
                return holdings.Values
                    .Where(h => h.Quantity > 0)
                    .OrderBy(h => h.StockSymbol)
                    .ToList();
            }
            catch (Exception)
            {
                // Return empty list on error to prevent application crash
                return new List<PortfolioHolding>();
            }
        }

        /// <summary>
        /// Gets the current quantity held for a specific stock
        /// </summary>
        public async Task<decimal> GetCurrentQuantityAsync(string stockSymbol)
        {
            try
            {
                return await GetQuantityAsOfDateAsync(stockSymbol, DateTime.Today);
            }
            catch (Exception)
            {
                // Return 0 on error to prevent application crash
                return 0;
            }
        }

        /// <summary>
        /// Gets the quantity held for a specific stock as of a date
        /// </summary>
        public async Task<decimal> GetQuantityAsOfDateAsync(string stockSymbol, DateTime date)
        {
            try
            {
                var trades = await _context.Trades
                    .Where(t => t.StockSymbol.ToUpper() == stockSymbol.ToUpper() && t.TradeDate <= date)
                    .OrderBy(t => t.TradeDate)
                    .ThenBy(t => t.CreatedAt)
                    .ToListAsync();

                decimal quantity = 0;

                foreach (var trade in trades)
                {
                    if (trade.Type == TradeType.Buy)
                    {
                        quantity += trade.Quantity;
                    }
                    else // Sell
                    {
                        quantity -= trade.Quantity;
                    }
                }

                return Math.Max(0, quantity); // Ensure we don't return negative quantities
            }
            catch (Exception)
            {
                // Return 0 on error to prevent application crash
                return 0;
            }
        }
    }
}