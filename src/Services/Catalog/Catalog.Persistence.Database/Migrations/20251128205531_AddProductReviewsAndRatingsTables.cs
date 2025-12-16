using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProductReviewsAndRatingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductRatings",
                schema: "Catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0m),
                    TotalReviews = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Rating5Star = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Rating4Star = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Rating3Star = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Rating2Star = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Rating1Star = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRatings", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_ProductRatings_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Catalog",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                schema: "Catalog",
                columns: table => new
                {
                    ReviewId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NotHelpfulCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.ReviewId);
                    table.CheckConstraint("CK_ProductReviews_Rating", "[Rating] >= 1.0 AND [Rating] <= 5.0");
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Catalog",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_Rating",
                schema: "Catalog",
                table: "ProductRatings",
                column: "AverageRating",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_CreatedAt",
                schema: "Catalog",
                table: "ProductReviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_Product_Rating",
                schema: "Catalog",
                table: "ProductReviews",
                columns: new[] { "ProductId", "Rating", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_User",
                schema: "Catalog",
                table: "ProductReviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductRatings",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "ProductReviews",
                schema: "Catalog");
        }
    }
}
