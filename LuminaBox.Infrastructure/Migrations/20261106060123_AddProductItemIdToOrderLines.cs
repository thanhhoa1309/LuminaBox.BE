using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductItemIdToOrderLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderBoxSelections_OrderItemId_ProductId_CategoryId_Filling~",
                table: "OrderBoxSelections");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductItemId",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductItemId",
                table: "OrderBoxSelections",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductItemId",
                table: "OrderItems",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBoxSelections_OrderItemId_ProductItemId",
                table: "OrderBoxSelections",
                columns: new[] { "OrderItemId", "ProductItemId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBoxSelections_ProductItemId",
                table: "OrderBoxSelections",
                column: "ProductItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderBoxSelections_ProductItems_ProductItemId",
                table: "OrderBoxSelections",
                column: "ProductItemId",
                principalTable: "ProductItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductItems_ProductItemId",
                table: "OrderItems",
                column: "ProductItemId",
                principalTable: "ProductItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderBoxSelections_ProductItems_ProductItemId",
                table: "OrderBoxSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductItems_ProductItemId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductItemId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderBoxSelections_OrderItemId_ProductItemId",
                table: "OrderBoxSelections");

            migrationBuilder.DropIndex(
                name: "IX_OrderBoxSelections_ProductItemId",
                table: "OrderBoxSelections");

            migrationBuilder.DropColumn(
                name: "ProductItemId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductItemId",
                table: "OrderBoxSelections");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBoxSelections_OrderItemId_ProductId_CategoryId_Filling~",
                table: "OrderBoxSelections",
                columns: new[] { "OrderItemId", "ProductId", "CategoryId", "FillingId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
