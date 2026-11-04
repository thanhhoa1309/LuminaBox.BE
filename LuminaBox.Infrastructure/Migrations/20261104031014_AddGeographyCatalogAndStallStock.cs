using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeographyCatalogAndStallStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CakeCategories_Organizers_OrganizerId",
                table: "CakeCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CakeCategories_CategoryId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Fillings_FillingId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Fillings_Organizers_OrganizerId",
                table: "Fillings");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ShippingAddresses_ShippingAddressId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "BoxImages");

            migrationBuilder.DropTable(
                name: "BoxItemProducts");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductStalls");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "ShippingAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Orders_GhnOrderCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Fillings_OrganizerId_Name",
                table: "Fillings");

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

            migrationBuilder.DropIndex(
                name: "IX_CakeCategories_OrganizerId_Name",
                table: "CakeCategories");

            migrationBuilder.DropColumn(
                name: "AddressDetail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhnOrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhnStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ToProvinceId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ToProvinceName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ToWardId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ToWardName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Fillings");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "FillingId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "CakeCategories");

            migrationBuilder.RenameColumn(
                name: "ShippingAddressId",
                table: "Orders",
                newName: "PickupAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders",
                newName: "IX_Orders_PickupAddressId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "CartItems",
                newName: "ProductItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_ProductItemId");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "Stalls",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"Stalls\" SET \"AddressLine\" = COALESCE(LEFT(\"Location\", 500), '');");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Stalls");

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "Stalls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Stalls",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Stalls",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapRefId",
                table: "Stalls",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewProvinceName",
                table: "Stalls",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewWardName",
                table: "Stalls",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                table: "Stalls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WardId",
                table: "Stalls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentType",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "Delivery");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Fillings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Fillings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CakeCategories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CakeCategories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BoxItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "BoxItems",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthCm",
                table: "BoxItems",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "BoxItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthCm",
                table: "BoxItems",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BoxItemImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoxItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxItemImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoxItemImages_BoxItems_BoxItemId",
                        column: x => x.BoxItemId,
                        principalTable: "BoxItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FillingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EggCount = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    LengthCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    WidthCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductItems", x => x.Id);
                    table.CheckConstraint("CK_ProductItems_Dimensions", "\"LengthCm\" > 0 AND \"WidthCm\" > 0 AND \"HeightCm\" > 0");
                    table.CheckConstraint("CK_ProductItems_EggCount", "\"EggCount\" >= 0");
                    table.CheckConstraint("CK_ProductItems_Price", "\"Price\" > 0");
                    table.CheckConstraint("CK_ProductItems_Weight", "\"Weight\" > 0");
                    table.ForeignKey(
                        name: "FK_ProductItems_CakeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CakeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductItems_Fillings_FillingId",
                        column: x => x.FillingId,
                        principalTable: "Fillings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GhnProvinceId = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StallBoxItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StallId = table.Column<Guid>(type: "uuid", nullable: true),
                    BoxItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StallBoxItems", x => x.Id);
                    table.CheckConstraint("CK_StallBoxItems_StockQuantity", "\"StockQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_StallBoxItems_BoxItems_BoxItemId",
                        column: x => x.BoxItemId,
                        principalTable: "BoxItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StallBoxItems_Stalls_StallId",
                        column: x => x.StallId,
                        principalTable: "Stalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    BoxItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    MinQuantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceTiers", x => x.Id);
                    table.CheckConstraint("CK_PriceTiers_ItemType_Target", "(\"ItemType\" = 'Product' AND \"ProductItemId\" IS NOT NULL AND \"BoxItemId\" IS NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductItemId\" IS NULL)");
                    table.CheckConstraint("CK_PriceTiers_MinQuantity", "\"MinQuantity\" >= 2");
                    table.CheckConstraint("CK_PriceTiers_UnitPrice", "\"UnitPrice\" > 0");
                    table.ForeignKey(
                        name: "FK_PriceTiers_BoxItems_BoxItemId",
                        column: x => x.BoxItemId,
                        principalTable: "BoxItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceTiers_ProductItems_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "ProductItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductItemImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductItemImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductItemImages_ProductItems_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "ProductItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StallProductItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StallId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StallProductItems", x => x.Id);
                    table.CheckConstraint("CK_StallProductItems_StockQuantity", "\"StockQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_StallProductItems_ProductItems_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "ProductItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StallProductItems_Stalls_StallId",
                        column: x => x.StallId,
                        principalTable: "Stalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GhnDistrictId = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GhnWardCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wards_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    WardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MapRefId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NewProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NewWardName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Wards_WardId",
                        column: x => x.WardId,
                        principalTable: "Wards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stalls_DistrictId",
                table: "Stalls",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Stalls_ProvinceId",
                table: "Stalls",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Stalls_WardId",
                table: "Stalls",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AddressId",
                table: "Orders",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Fillings_Name",
                table: "Fillings",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductItemId_Online",
                table: "CartItems",
                columns: new[] { "CartId", "ProductItemId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductItemId\" IS NOT NULL AND \"StallId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductItemId_StallId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductItemId", "StallId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductItemId\" IS NOT NULL AND \"StallId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItems_ItemType_Target",
                table: "CartItems",
                sql: "(\"ItemType\" = 'Product' AND \"ProductItemId\" IS NOT NULL AND \"BoxItemId\" IS NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductItemId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_CakeCategories_Name",
                table: "CakeCategories",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_DistrictId",
                table: "Addresses",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ProvinceId",
                table: "Addresses",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_WardId",
                table: "Addresses",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "UX_Addresses_User_Type_Default",
                table: "Addresses",
                columns: new[] { "UserId", "Type" },
                unique: true,
                filter: "\"IsDefault\" = true AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BoxItemImages_BoxItemId_SortOrder",
                table: "BoxItemImages",
                columns: new[] { "BoxItemId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Districts_ProvinceId_Code",
                table: "Districts",
                columns: new[] { "ProvinceId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTiers_BoxItemId_MinQuantity",
                table: "PriceTiers",
                columns: new[] { "BoxItemId", "MinQuantity" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"BoxItemId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTiers_ProductItemId_MinQuantity",
                table: "PriceTiers",
                columns: new[] { "ProductItemId", "MinQuantity" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProductItemId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItemImages_ProductItemId_SortOrder",
                table: "ProductItemImages",
                columns: new[] { "ProductItemId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_CategoryId",
                table: "ProductItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_FillingId",
                table: "ProductItems",
                column: "FillingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_ProductId_CategoryId_FillingId_EggCount_Weight",
                table: "ProductItems",
                columns: new[] { "ProductId", "CategoryId", "FillingId", "EggCount", "Weight" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_Code",
                table: "Provinces",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StallBoxItems_StallId_BoxItemId",
                table: "StallBoxItems",
                columns: new[] { "StallId", "BoxItemId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StallId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_StallBoxItems_BoxItem_Online",
                table: "StallBoxItems",
                column: "BoxItemId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StallId\" IS NULL");

            migrationBuilder.Sql("INSERT INTO \"StallBoxItems\" (\"Id\", \"StallId\", \"BoxItemId\", \"StockQuantity\", \"IsActive\", \"IsDeleted\", \"CreatedAt\") SELECT gen_random_uuid(), \"StallId\", \"BoxItemId\", 0, true, false, now() FROM \"BoxItemStalls\";");

            migrationBuilder.DropTable(
                name: "BoxItemStalls");

            migrationBuilder.CreateIndex(
                name: "IX_StallProductItems_StallId_ProductItemId",
                table: "StallProductItems",
                columns: new[] { "StallId", "ProductItemId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StallId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_StallProductItems_ProductItem_Online",
                table: "StallProductItems",
                column: "ProductItemId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StallId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_DistrictId_Code",
                table: "Wards",
                columns: new[] { "DistrictId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductItems_ProductItemId",
                table: "CartItems",
                column: "ProductItemId",
                principalTable: "ProductItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_AddressId",
                table: "Orders",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_PickupAddressId",
                table: "Orders",
                column: "PickupAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stalls_Districts_DistrictId",
                table: "Stalls",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stalls_Provinces_ProvinceId",
                table: "Stalls",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stalls_Wards_WardId",
                table: "Stalls",
                column: "WardId",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductItems_ProductItemId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_AddressId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_PickupAddressId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Stalls_Districts_DistrictId",
                table: "Stalls");

            migrationBuilder.DropForeignKey(
                name: "FK_Stalls_Provinces_ProvinceId",
                table: "Stalls");

            migrationBuilder.DropForeignKey(
                name: "FK_Stalls_Wards_WardId",
                table: "Stalls");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "BoxItemImages");

            migrationBuilder.DropTable(
                name: "PriceTiers");

            migrationBuilder.DropTable(
                name: "ProductItemImages");

            migrationBuilder.DropTable(
                name: "StallBoxItems");

            migrationBuilder.DropTable(
                name: "StallProductItems");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "ProductItems");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_Stalls_DistrictId",
                table: "Stalls");

            migrationBuilder.DropIndex(
                name: "IX_Stalls_ProvinceId",
                table: "Stalls");

            migrationBuilder.DropIndex(
                name: "IX_Stalls_WardId",
                table: "Stalls");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Fillings_Name",
                table: "Fillings");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductItemId_Online",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductItemId_StallId",
                table: "CartItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItems_ItemType_Target",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CakeCategories_Name",
                table: "CakeCategories");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Stalls",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"Stalls\" SET \"Location\" = NULLIF(\"AddressLine\", '');");

            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "MapRefId",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "NewProvinceName",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "NewWardName",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FulfillmentType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Fillings");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "BoxItems");

            migrationBuilder.DropColumn(
                name: "LengthCm",
                table: "BoxItems");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "BoxItems");

            migrationBuilder.DropColumn(
                name: "WidthCm",
                table: "BoxItems");

            migrationBuilder.RenameColumn(
                name: "PickupAddressId",
                table: "Orders",
                newName: "ShippingAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_PickupAddressId",
                table: "Orders",
                newName: "IX_Orders_ShippingAddressId");

            migrationBuilder.RenameColumn(
                name: "ProductItemId",
                table: "CartItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ProductItemId",
                table: "CartItems",
                newName: "IX_CartItems_ProductId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDetail",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<int>(
                name: "ToProvinceId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ToProvinceName",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ToWardId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ToWardName",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Fillings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizerId",
                table: "Fillings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CakeCategories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CakeCategories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizerId",
                table: "CakeCategories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BoxItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "BoxImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoxItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoxImages_BoxItems_BoxItemId",
                        column: x => x.BoxItemId,
                        principalTable: "BoxItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoxItemProducts",
                columns: table => new
                {
                    BoxItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxItemProducts", x => new { x.BoxItemId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_BoxItemProducts_BoxItems_BoxItemId",
                        column: x => x.BoxItemId,
                        principalTable: "BoxItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoxItemProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoxItemStalls",
                columns: table => new
                {
                    BoxItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StallId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxItemStalls", x => new { x.BoxItemId, x.StallId });
                    table.ForeignKey(
                        name: "FK_BoxItemStalls_BoxItems_BoxItemId",
                        column: x => x.BoxItemId,
                        principalTable: "BoxItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoxItemStalls_Stalls_StallId",
                        column: x => x.StallId,
                        principalTable: "Stalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductStalls",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    StallId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStalls", x => new { x.ProductId, x.StallId });
                    table.ForeignKey(
                        name: "FK_ProductStalls_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductStalls_Stalls_StallId",
                        column: x => x.StallId,
                        principalTable: "Stalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FillingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.CheckConstraint("CK_ProductVariants_Price", "\"Price\" > 0");
                    table.ForeignKey(
                        name: "FK_ProductVariants_CakeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CakeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Fillings_FillingId",
                        column: x => x.FillingId,
                        principalTable: "Fillings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShippingAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressDetail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "text", nullable: false),
                    ReceiverName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiverPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ToProvinceId = table.Column<int>(type: "integer", nullable: false),
                    ToProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToWardId = table.Column<int>(type: "integer", nullable: false),
                    ToWardName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GhnOrderCode",
                table: "Orders",
                column: "GhnOrderCode",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Fillings_OrganizerId_Name",
                table: "Fillings",
                columns: new[] { "OrganizerId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

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

            migrationBuilder.CreateIndex(
                name: "IX_CakeCategories_OrganizerId_Name",
                table: "CakeCategories",
                columns: new[] { "OrganizerId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BoxImages_BoxItemId",
                table: "BoxImages",
                column: "BoxItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxImages_BoxItemId_IsPrimary",
                table: "BoxImages",
                columns: new[] { "BoxItemId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_BoxItemProducts_ProductId",
                table: "BoxItemProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxItemStalls_StallId",
                table: "BoxItemStalls",
                column: "StallId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStalls_StallId",
                table: "ProductStalls",
                column: "StallId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_CategoryId",
                table: "ProductVariants",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_FillingId",
                table: "ProductVariants",
                column: "FillingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_CategoryId_FillingId",
                table: "ProductVariants",
                columns: new[] { "ProductId", "CategoryId", "FillingId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingAddresses_OwnerType_OwnerId_IsDefault",
                table: "ShippingAddresses",
                columns: new[] { "OwnerType", "OwnerId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "UX_ShippingAddresses_Owner_Single",
                table: "ShippingAddresses",
                columns: new[] { "OwnerType", "OwnerId" },
                unique: true,
                filter: "\"OwnerType\" IN ('Organizer', 'Stall') AND \"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_CakeCategories_Organizers_OrganizerId",
                table: "CakeCategories",
                column: "OrganizerId",
                principalTable: "Organizers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fillings_Organizers_OrganizerId",
                table: "Fillings",
                column: "OrganizerId",
                principalTable: "Organizers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ShippingAddresses_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId",
                principalTable: "ShippingAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
