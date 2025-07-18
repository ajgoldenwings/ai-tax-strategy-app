# Design Document

## Overview

The Stock Portfolio Tracker is a .NET web application built using ASP.NET Core MVC that allows users to track their personal stock holdings and trade history over time. The application provides a clean, responsive interface using Bootstrap CSS for styling and maintains persistent data storage to track portfolio changes across different dates.

The system follows a traditional MVC architecture pattern with clear separation of concerns between data models, business logic, and presentation layers. The application will use Entity Framework Core for data persistence with SQLite as the database provider for simplicity and portability.

## Architecture

The application follows the Model-View-Controller (MVC) pattern with the following architectural layers:

### Presentation Layer (Views)
- Razor views with Bootstrap CSS styling
- Responsive design for desktop and mobile devices
- Client-side validation using jQuery and Bootstrap validation classes
- No inline CSS - all styling through Bootstrap classes

### Controller Layer
- HomeController for main portfolio views
- TradesController for trade management operations
- API controllers for AJAX operations if needed
- Input validation and error handling

### Business Logic Layer (Services)
- PortfolioService for calculating holdings and historical positions
- TradeService for trade operations and validation
- ValidationService for business rule validation

### Data Access Layer (Repositories)
- Entity Framework Core with Code First approach
- Repository pattern for data access abstraction
- SQLite database for local data storage

### Data Models
- Trade entity for individual transactions
- Portfolio calculation models for current and historical holdings
- ViewModels for UI data binding

## Components and Interfaces

### Core Models

#### Trade Model
```csharp
public class Trade
{
    public int Id { get; set; }
    public string StockSymbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public TradeType Type { get; set; } // Buy/Sell enum
    public DateTime TradeDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### Portfolio Holding Model
```csharp
public class PortfolioHolding
{
    public string StockSymbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCostBasis { get; set; }
    public decimal TotalValue { get; set; }
}
```

### Services

#### IPortfolioService
- CalculateCurrentHoldings(): Returns current portfolio state
- CalculateHoldingsAsOfDate(DateTime date): Returns historical portfolio state
- GetPortfolioHistory(): Returns portfolio changes over time

#### ITradeService
- AddTrade(Trade trade): Adds new trade with validation
- GetAllTrades(): Returns all trades chronologically
- ValidateTrade(Trade trade): Validates trade data

### Controllers

#### HomeController
- Index(): Dashboard with current portfolio overview
- History(): Portfolio history view with date selection
- Privacy(): Privacy policy page

#### TradesController
- Index(): List all trades
- Create(): Add new trade form
- Details(int id): View trade details
- Delete(int id): Delete trade (with confirmation)

### Views Structure

#### Shared Layout
- Bootstrap navigation bar
- Responsive container layout
- Footer with application info

#### Home Views
- Index.cshtml: Current portfolio dashboard
- History.cshtml: Historical portfolio viewer with date picker

#### Trades Views
- Index.cshtml: Trade history table
- Create.cshtml: Add trade form
- Details.cshtml: Trade details view
- Delete.cshtml: Delete confirmation

## Data Models

### Database Schema

#### Trades Table
- Id (Primary Key, Auto-increment)
- StockSymbol (VARCHAR(10), Required)
- Quantity (DECIMAL(18,4), Required, > 0)
- Price (DECIMAL(18,4), Required, > 0)
- Type (INT, Required) // 0=Buy, 1=Sell
- TradeDate (DATETIME, Required)
- CreatedAt (DATETIME, Required)

### Entity Framework Configuration
- DbContext with Trades DbSet
- Fluent API configuration for decimal precision
- Seed data for development/testing
- Automatic migrations enabled

### Business Logic Models

#### Portfolio Calculation Logic
The system calculates portfolio holdings by:
1. Retrieving all trades up to the specified date
2. Grouping trades by stock symbol
3. Calculating net position (buys - sells) for each stock
4. Computing average cost basis using weighted average method
5. Filtering out stocks with zero or negative positions

## Error Handling

### Validation Rules
- Stock symbols must be 1-10 characters, alphanumeric
- Quantities must be positive decimal numbers
- Prices must be positive decimal numbers
- Trade dates cannot be in the future
- Sell trades cannot exceed current holdings

### Error Display Strategy
- Model validation errors displayed using Bootstrap alert components
- Client-side validation for immediate feedback
- Server-side validation as final check
- Friendly error messages for business rule violations
- Logging of system errors for debugging

### Exception Handling
- Global exception handler for unhandled exceptions
- Try-catch blocks around database operations
- Graceful degradation for data access failures
- User-friendly error pages

## Testing Strategy

There will be not testing.

## User Interface Design

### Bootstrap Components Used
- Navigation bar for main menu
- Cards for portfolio summary display
- Tables for trade history and holdings
- Forms with validation styling
- Buttons with appropriate styling classes
- Alerts for error and success messages
- Date picker for historical date selection

### Responsive Design
- Mobile-first approach using Bootstrap grid system
- Collapsible navigation for mobile devices
- Responsive tables that stack on small screens
- Touch-friendly button sizes

### User Experience Flow
1. Landing page shows current portfolio overview
2. Easy navigation to add new trades
3. Historical view accessible from main navigation
4. Clear visual feedback for all user actions
5. Consistent styling throughout application

## Performance Considerations

### Database Optimization
- Indexes on TradeDate and StockSymbol columns
- Efficient queries for portfolio calculations
- Pagination for large trade lists

### Caching Strategy
- Cache current portfolio calculations
- Cache historical calculations for frequently accessed dates
- Use memory cache for session-based data

### Scalability Notes
- Repository pattern allows for easy database provider changes
- Service layer can be extracted to separate projects if needed
- API endpoints can be added for mobile app integration