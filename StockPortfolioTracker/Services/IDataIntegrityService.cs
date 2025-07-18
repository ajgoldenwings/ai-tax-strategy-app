namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Interface for data integrity checking and validation
    /// </summary>
    public interface IDataIntegrityService
    {
        /// <summary>
        /// Performs comprehensive data integrity check
        /// </summary>
        Task<DataIntegrityResult> CheckDataIntegrityAsync();

        /// <summary>
        /// Validates database connection and basic operations
        /// </summary>
        Task<bool> ValidateDatabaseConnectionAsync();

        /// <summary>
        /// Checks for corrupted trade data
        /// </summary>
        Task<IEnumerable<string>> CheckForCorruptedDataAsync();

        /// <summary>
        /// Attempts to repair minor data corruption issues
        /// </summary>
        Task<bool> RepairDataCorruptionAsync();
    }

    /// <summary>
    /// Result of data integrity check
    /// </summary>
    public class DataIntegrityResult
    {
        public bool IsHealthy { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public int TotalTrades { get; set; }
        public int CorruptedTrades { get; set; }
        public DateTime LastChecked { get; set; }
    }
}