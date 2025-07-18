using Microsoft.AspNetCore.Mvc;
using StockPortfolioTracker.Models;
using StockPortfolioTracker.Services;

namespace StockPortfolioTracker.Controllers
{
    /// <summary>
    /// Controller for managing stock trades
    /// </summary>
    public class TradesController : Controller
    {
        private readonly ITradeService _tradeService;

        public TradesController(ITradeService tradeService)
        {
            _tradeService = tradeService;
        }

        /// <summary>
        /// Display all trades in chronological order
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var trades = await _tradeService.GetAllTradesAsync();
                return View(trades);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to load trade data. Please try again.";
                return View(new List<Trade>());
            }
        }

        /// <summary>
        /// Display form for creating a new trade
        /// </summary>
        public IActionResult Create()
        {
            var trade = new Trade
            {
                TradeDate = DateTime.Today
            };
            return View(trade);
        }

        /// <summary>
        /// Process the creation of a new trade
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trade trade)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var validationResult = await _tradeService.ValidateTradeAsync(trade);
                    
                    if (validationResult.IsValid)
                    {
                        var success = await _tradeService.AddTradeAsync(trade);
                        if (success)
                        {
                            var successMessage = $"Trade added successfully! {trade.Type} {trade.Quantity:N4} shares of {trade.StockSymbol} at ${trade.Price:N2} per share.";
                            
                            // Add any warnings to the success message
                            if (validationResult.WarningMessages.Any())
                            {
                                TempData["WarningMessage"] = string.Join(" ", validationResult.WarningMessages);
                            }
                            
                            TempData["SuccessMessage"] = successMessage;
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to save the trade. This may be due to a database error or network issue. Please try again.");
                        }
                    }
                    else
                    {
                        foreach (var error in validationResult.ErrorMessages)
                        {
                            ModelState.AddModelError("", error);
                        }
                        
                        // Display warnings even when validation fails
                        if (validationResult.WarningMessages.Any())
                        {
                            foreach (var warning in validationResult.WarningMessages)
                            {
                                ModelState.AddModelError("", $"Warning: {warning}");
                            }
                        }
                    }
                }
                else
                {
                    // Add a general message for model validation errors
                    if (!ModelState.Values.SelectMany(v => v.Errors).Any())
                    {
                        ModelState.AddModelError("", "Please correct the highlighted fields and try again.");
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred while processing your trade. Please try again.");
            }

            return View(trade);
        }

        /// <summary>
        /// Display details for a specific trade
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var trades = await _tradeService.GetAllTradesAsync();
                var trade = trades.FirstOrDefault(t => t.Id == id);
                
                if (trade == null)
                {
                    TempData["ErrorMessage"] = "The requested trade could not be found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(trade);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to load trade details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Display confirmation page for deleting a trade
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var trades = await _tradeService.GetAllTradesAsync();
                var trade = trades.FirstOrDefault(t => t.Id == id);
                
                if (trade == null)
                {
                    TempData["ErrorMessage"] = "The requested trade could not be found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(trade);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to load trade for deletion. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Process the deletion of a trade
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _tradeService.DeleteTradeAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Trade deleted successfully. Your portfolio has been updated.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete trade. The trade may not exist or a database error occurred. Please try again.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the trade. Please try again.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}