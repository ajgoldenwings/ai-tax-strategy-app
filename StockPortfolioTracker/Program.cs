using Microsoft.EntityFrameworkCore;
using StockPortfolioTracker.Data;
using StockPortfolioTracker.Services;
using StockPortfolioTracker.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add TempData support for error handling
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add business services
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddScoped<IDataIntegrityService, DataIntegrityService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();

var app = builder.Build();

// Ensure database is created and migrations are applied with data integrity checks
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var dataIntegrityService = scope.ServiceProvider.GetRequiredService<IDataIntegrityService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Perform comprehensive data integrity check using the service
        var integrityResult = await dataIntegrityService.CheckDataIntegrityAsync();
        
        if (integrityResult.IsHealthy)
        {
            logger.LogInformation("Database integrity check passed. Found {TradeCount} trades.", integrityResult.TotalTrades);
        }
        else
        {
            logger.LogWarning("Database integrity issues detected: {IssueCount} issues, {WarningCount} warnings", 
                integrityResult.Issues.Count, integrityResult.Warnings.Count);
            
            foreach (var issue in integrityResult.Issues)
            {
                logger.LogWarning("Data integrity issue: {Issue}", issue);
            }
            
            // Attempt to repair minor issues automatically
            if (integrityResult.CorruptedTrades > 0)
            {
                logger.LogInformation("Attempting to repair data corruption issues...");
                var repairSuccess = await dataIntegrityService.RepairDataCorruptionAsync();
                
                if (repairSuccess)
                {
                    logger.LogInformation("Data corruption repair completed successfully.");
                }
                else
                {
                    logger.LogError("Failed to repair data corruption. Manual intervention may be required.");
                }
            }
        }
        
        // Seed data for development and demonstration purposes
        if (app.Environment.IsDevelopment())
        {
            var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
            
            try
            {
                await seedDataService.SeedDataAsync();
                logger.LogInformation("Seed data initialization completed successfully.");
            }
            catch (Exception seedEx)
            {
                logger.LogError(seedEx, "An error occurred while seeding data.");
                // Don't throw here as seed data is not critical for application startup
            }
        }
        
        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        
        // In production, you might want to exit the application if database cannot be initialized
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}



// Configure the HTTP request pipeline.
// Use custom global exception handling middleware in all environments
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
