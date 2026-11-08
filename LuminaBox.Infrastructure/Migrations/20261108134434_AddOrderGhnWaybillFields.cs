using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderGhnWaybillFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GhnOrderCode",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhnStatus",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GhnOrderCode",
                table: "Orders",
                column: "GhnOrderCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_GhnOrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhnOrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhnStatus",
                table: "Orders");
        }
    }
}
