namespace StockPortfolioTracker.Services
{
    /// <summary>
    /// Interface for seed data operations
    /// </summary>
    public interface ISeedDataService
    {
        /// <summary>
        /// Seeds the database with sample data for development and demonstration
        /// </summary>
        Task SeedDataAsync();

        /// <summary>
        /// Checks if seed data already exists
        /// </summary>
        Task<bool> HasSeedDataAsync();

        /// <summary>
        /// Clears all seed data from the database
        /// </summary>
        Task ClearSeedDataAsync();
    }
}