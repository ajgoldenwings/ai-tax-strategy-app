namespace StockPortfolioTracker.Models
{
    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public List<string> WarningMessages { get; set; } = new List<string>();
    }
}