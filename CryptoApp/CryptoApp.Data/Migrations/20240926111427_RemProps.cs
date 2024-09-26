using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "InitialValue",
                table: "Currencies");

            migrationBuilder.AlterColumn<double>(
                name: "ChangePercentage",
                table: "Currencies",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "ChangePercentage",
                table: "Currencies",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentValue",
                table: "Currencies",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialValue",
                table: "Currencies",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
