using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockPortfolioTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StockSymbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_StockSymbol",
                table: "Trades",
                column: "StockSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_StockSymbol_TradeDate",
                table: "Trades",
                columns: new[] { "StockSymbol", "TradeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TradeDate",
                table: "Trades",
                column: "TradeDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
