using System.ComponentModel.DataAnnotations;

namespace StockPortfolioTracker.Models
{
    /// <summary>
    /// Represents a stock trade transaction
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Unique identifier for the trade
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Stock symbol (e.g., AAPL, MSFT)
        /// </summary>
        [Required(ErrorMessage = "Stock symbol is required")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Stock symbol must be between 1 and 10 characters")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Stock symbol must contain only alphanumeric characters")]
        [Display(Name = "Stock Symbol")]
        public string StockSymbol { get; set; } = string.Empty;

        /// <summary>
        /// Number of shares traded
        /// </summary>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Price per share
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price per Share")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }

        /// <summary>
        /// Type of trade (Buy or Sell)
        /// </summary>
        [Required(ErrorMessage = "Trade type is required")]
        [Display(Name = "Trade Type")]
        public TradeType Type { get; set; }

        /// <summary>
        /// Date when the trade was executed
        /// </summary>
        [Required(ErrorMessage = "Trade date is required")]
        [Display(Name = "Trade Date")]
        [DataType(DataType.Date)]
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// Timestamp when the trade record was created in the system
        /// </summary>
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Calculated total value of the trade
        /// </summary>
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalValue => Quantity * Price;
    }
}