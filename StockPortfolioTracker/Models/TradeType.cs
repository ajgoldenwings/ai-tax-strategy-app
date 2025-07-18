namespace StockPortfolioTracker.Models
{
    /// <summary>
    /// Represents the type of trade operation
    /// </summary>
    public enum TradeType
    {
        /// <summary>
        /// Buy operation - acquiring stocks
        /// </summary>
        Buy = 0,
        
        /// <summary>
        /// Sell operation - disposing stocks
        /// </summary>
        Sell = 1
    }
}