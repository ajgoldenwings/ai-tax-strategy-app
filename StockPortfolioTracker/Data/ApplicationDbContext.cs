using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Models;

namespace StockPortfolioTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet for Trade entities
        /// </summary>
        public DbSet<Trade> Trades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Trade entity
            modelBuilder.Entity<Trade>(entity =>
            {
                // Primary key
                entity.HasKey(t => t.Id);

                // Configure StockSymbol
                entity.Property(t => t.StockSymbol)
                    .IsRequired()
                    .HasMaxLength(10);

                // Configure Quantity with precision
                entity.Property(t => t.Quantity)
                    .IsRequired()
                    .HasColumnType("decimal(18,4)");

                // Configure Price with precision
                entity.Property(t => t.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,4)");

                // Configure TradeType enum
                entity.Property(t => t.Type)
                    .IsRequired()
                    .HasConversion<int>();

                // Configure TradeDate
                entity.Property(t => t.TradeDate)
                    .IsRequired()
                    .HasColumnType("datetime");

                // Configure CreatedAt
                entity.Property(t => t.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("datetime('now')");

                // Create indexes for better query performance
                entity.HasIndex(t => t.StockSymbol)
                    .HasDatabaseName("IX_Trades_StockSymbol");

                entity.HasIndex(t => t.TradeDate)
                    .HasDatabaseName("IX_Trades_TradeDate");

                entity.HasIndex(t => new { t.StockSymbol, t.TradeDate })
                    .HasDatabaseName("IX_Trades_StockSymbol_TradeDate");

                // Additional composite index for portfolio calculations
                entity.HasIndex(t => new { t.TradeDate, t.StockSymbol, t.Type })
                    .HasDatabaseName("IX_Trades_TradeDate_StockSymbol_Type");
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically set CreatedAt timestamp
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically set CreatedAt timestamp
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates CreatedAt timestamp for new entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<Trade>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}