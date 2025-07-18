using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StockPortfolioTracker.Middleware
{
    /// <summary>
    /// Global exception handling middleware for unhandled exceptions
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. Request Path: {Path}, User: {User}, TraceId: {TraceId}", 
                    context.Request.Path, 
                    context.User?.Identity?.Name ?? "Anonymous", 
                    context.TraceIdentifier);
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Don't modify response if it has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Cannot handle exception - response has already started");
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = GetStatusCode(exception);

            // Store exception details in TempData for the error page
            if (context.RequestServices.GetService<ITempDataProvider>() != null)
            {
                var tempDataFactory = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
                var tempData = tempDataFactory.GetTempData(context);
                
                tempData["ErrorMessage"] = GetUserFriendlyMessage(exception);
                if (_environment.IsDevelopment())
                {
                    tempData["ExceptionDetails"] = exception.ToString();
                }
            }

            // For AJAX requests, return JSON response
            if (IsAjaxRequest(context.Request))
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = GetUserFriendlyMessage(exception),
                    TraceId = context.TraceIdentifier
                };

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
                return;
            }

            // For regular web requests, redirect to error page
            context.Response.Redirect("/Home/Error");
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentException => "Invalid request parameters provided.",
                UnauthorizedAccessException => "You are not authorized to perform this action.",
                NotImplementedException => "This feature is not yet implemented.",
                InvalidOperationException => "The requested operation could not be completed.",
                _ => "An unexpected error occurred while processing your request."
            };
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}