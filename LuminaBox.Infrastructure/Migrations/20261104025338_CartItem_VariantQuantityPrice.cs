using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CartItem_VariantQuantityPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId_Online",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId_StallId",
                table: "CartItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItems_ItemType_Target",
                table: "CartItems");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "CartItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FillingId",
                table: "CartItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "CartItems",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 1m);

            // Old cart lines lack Category/Filling; wipe before stricter constraints.
            migrationBuilder.Sql("DELETE FROM \"CartItems\";");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_Product_Variant_Online",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId", "CategoryId", "FillingId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductId\" IS NOT NULL AND \"StallId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_Product_Variant_Stall",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId", "CategoryId", "FillingId", "StallId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductId\" IS NOT NULL AND \"StallId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CategoryId",
                table: "CartItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_FillingId",
                table: "CartItems",
                column: "FillingId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItems_ItemType_Target",
                table: "CartItems",
                sql: "(\"ItemType\" = 'Product' AND \"ProductId\" IS NOT NULL AND \"BoxItemId\" IS NULL AND \"CategoryId\" IS NOT NULL AND \"FillingId\" IS NOT NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductId\" IS NULL AND \"CategoryId\" IS NULL AND \"FillingId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItems_Quantity",
                table: "CartItems",
                sql: "\"Quantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItems_UnitPrice",
                table: "CartItems",
                sql: "\"UnitPrice\" > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_CakeCategories_CategoryId",
                table: "CartItems",
                column: "CategoryId",
                principalTable: "CakeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Fillings_FillingId",
                table: "CartItems",
                column: "FillingId",
                principalTable: "Fillings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CakeCategories_CategoryId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Fillings_FillingId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_Product_Variant_Online",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_Product_Variant_Stall",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CategoryId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_FillingId",
                table: "CartItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItems_ItemType_Target",
                table: "CartItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItems_Quantity",
                table: "CartItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItems_UnitPrice",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "FillingId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId_Online",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductId\" IS NOT NULL AND \"StallId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId_StallId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId", "StallId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductId\" IS NOT NULL AND \"StallId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItems_ItemType_Target",
                table: "CartItems",
                sql: "(\"ItemType\" = 'Product' AND \"ProductId\" IS NOT NULL AND \"BoxItemId\" IS NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductId\" IS NULL)");
        }
    }
}
