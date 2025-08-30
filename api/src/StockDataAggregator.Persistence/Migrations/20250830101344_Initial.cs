using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockDataAggregator.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialsYearly",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    Earnings = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialsYearly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SymbolMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OneYearSalesGrowth = table.Column<decimal>(type: "numeric", nullable: false),
                    FourYearSalesGrowth = table.Column<decimal>(type: "numeric", nullable: false),
                    FourYearEarningsGrowth = table.Column<decimal>(type: "numeric", nullable: false),
                    FreeCashFlow = table.Column<decimal>(type: "numeric", nullable: false),
                    DebtToEquity = table.Column<decimal>(type: "numeric", nullable: false),
                    PegRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    ReturnOnEquity = table.Column<decimal>(type: "numeric", nullable: false),
                    DividendYield = table.Column<decimal>(type: "numeric", nullable: true),
                    EsgTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    EsgEnvironment = table.Column<decimal>(type: "numeric", nullable: false),
                    EsgSocial = table.Column<decimal>(type: "numeric", nullable: false),
                    EsgGovernance = table.Column<decimal>(type: "numeric", nullable: false),
                    EsgPublicationDate = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymbolMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackedSymbols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedSymbols", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialsYearly_Symbol_Year",
                table: "FinancialsYearly",
                columns: new[] { "Symbol", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialsYearly");

            migrationBuilder.DropTable(
                name: "SymbolMetrics");

            migrationBuilder.DropTable(
                name: "TrackedSymbols");
        }
    }
}
