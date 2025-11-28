using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índice compuesto para búsqueda de texto
            migrationBuilder.CreateIndex(
                name: "IX_Products_Search_Text",
                table: "Products",
                columns: new[] { "IsActive", "NameSpanish", "NameEnglish", "Brand", "SKU" });

            // Índice para filtrado por precio
            migrationBuilder.CreateIndex(
                name: "IX_Products_Price_Active",
                table: "Products",
                columns: new[] { "IsActive", "Price" });

            // Índice para productos destacados
            migrationBuilder.CreateIndex(
                name: "IX_Products_Featured_Active",
                table: "Products",
                columns: new[] { "IsActive", "IsFeatured", "CreatedAt" });

            // Índice filtrado para descuentos
            migrationBuilder.Sql(
                "CREATE NONCLUSTERED INDEX IX_Products_Discount_Active " +
                "ON Products (IsActive, DiscountPercentage) " +
                "WHERE DiscountPercentage > 0");

            // Índice para marca
            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand_Active",
                table: "Products",
                columns: new[] { "IsActive", "Brand" });

            // Índice en ProductCategories para filtro por categoría
            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryId_ProductId",
                table: "ProductCategories",
                columns: new[] { "CategoryId", "ProductId" });

            // Índice en ProductInStock para filtro de stock
            migrationBuilder.Sql(
                "CREATE NONCLUSTERED INDEX IX_ProductInStock_Stock " +
                "ON ProductInStock (ProductId, Stock) " +
                "WHERE Stock > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices en orden inverso
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_ProductInStock_Stock ON ProductInStock");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_CategoryId_ProductId",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Products_Brand_Active",
                table: "Products");

            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Products_Discount_Active ON Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Featured_Active",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Price_Active",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Search_Text",
                table: "Products");
        }
    }
}
