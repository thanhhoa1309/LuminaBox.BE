using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAndWalletConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Wallets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StallProductItems",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StallBoxItems",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "StallProductItems");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "StallBoxItems");
        }
    }
}
