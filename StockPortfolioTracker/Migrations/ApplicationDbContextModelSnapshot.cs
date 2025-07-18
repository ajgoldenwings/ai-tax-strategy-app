﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StockPortfolioTracker.Data;

#nullable disable

namespace StockPortfolioTracker.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.7");

            modelBuilder.Entity("StockPortfolioTracker.Models.Trade", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,4)");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("StockSymbol")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TradeDate")
                        .HasColumnType("datetime");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("StockSymbol")
                        .HasDatabaseName("IX_Trades_StockSymbol");

                    b.HasIndex("TradeDate")
                        .HasDatabaseName("IX_Trades_TradeDate");

                    b.HasIndex("StockSymbol", "TradeDate")
                        .HasDatabaseName("IX_Trades_StockSymbol_TradeDate");

                    b.ToTable("Trades");
                });
#pragma warning restore 612, 618
        }
    }
}
