using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaBox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStallRentalOverlapGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Service checks alone cannot stop two concurrent approvals from renting the same period
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                ALTER TABLE "StallRentals"
                ADD CONSTRAINT "EX_StallRentals_NoOverlap"
                EXCLUDE USING gist (
                    "StallId" WITH =,
                    daterange("StartDate", "EndDate", '[]') WITH &&
                )
                WHERE ("Status" IN ('Confirmed', 'Active') AND "IsDeleted" = false);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "StallRentals" DROP CONSTRAINT "EX_StallRentals_NoOverlap";""");
        }
    }
}
