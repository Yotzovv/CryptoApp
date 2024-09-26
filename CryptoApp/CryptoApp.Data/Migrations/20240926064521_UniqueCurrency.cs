using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Coin",
                table: "Currencies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Coin_PortfolioId",
                table: "Currencies",
                columns: new[] { "Coin", "PortfolioId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Currencies_Coin_PortfolioId",
                table: "Currencies");

            migrationBuilder.AlterColumn<string>(
                name: "Coin",
                table: "Currencies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
