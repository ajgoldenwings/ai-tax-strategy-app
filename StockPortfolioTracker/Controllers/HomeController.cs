using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockPortfolioTracker.Models;
using StockPortfolioTracker.Services;

namespace StockPortfolioTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPortfolioService _portfolioService;

    public HomeController(ILogger<HomeController> logger, IPortfolioService portfolioService)
    {
        _logger = logger;
        _portfolioService = portfolioService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var holdings = await _portfolioService.CalculateCurrentHoldingsAsync();
            return View(holdings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating current portfolio holdings");
            TempData["ErrorMessage"] = "Unable to load portfolio data. Please try again.";
            return View(new List<PortfolioHolding>());
        }
    }

    public async Task<IActionResult> History(DateTime? date)
    {
        try
        {
            // Default to today if no date is provided
            var selectedDate = date ?? DateTime.Today;
            
            // Ensure the date is not in the future
            if (selectedDate > DateTime.Today)
            {
                selectedDate = DateTime.Today;
                TempData["WarningMessage"] = "Future dates are not allowed. Showing portfolio for today.";
            }

            var holdings = await _portfolioService.CalculateHoldingsAsOfDateAsync(selectedDate);
            
            ViewBag.SelectedDate = selectedDate;
            ViewBag.FormattedDate = selectedDate.ToString("yyyy-MM-dd");
            
            return View(holdings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating historical portfolio holdings for date {Date}", date);
            TempData["ErrorMessage"] = "Unable to load historical portfolio data. Please try again.";
            return View(new List<PortfolioHolding>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Handle 404 Not Found errors
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public new IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View();
    }
}
