using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAttributesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                schema: "Catalog",
                columns: table => new
                {
                    AttributeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttributeNameEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttributeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsFilterable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.AttributeId);
                    table.ForeignKey(
                        name: "FK_ProductAttributes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Catalog",
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AttributeValues",
                schema: "Catalog",
                columns: table => new
                {
                    ValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    ValueText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValueTextEnglish = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeValues", x => x.ValueId);
                    table.ForeignKey(
                        name: "FK_AttributeValues_ProductAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalSchema: "Catalog",
                        principalTable: "ProductAttributes",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributeValues",
                schema: "Catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    ValueId = table.Column<int>(type: "int", nullable: false),
                    TextValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BooleanValue = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeValues", x => new { x.ProductId, x.AttributeId, x.ValueId });
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_AttributeValues_ValueId",
                        column: x => x.ValueId,
                        principalSchema: "Catalog",
                        principalTable: "AttributeValues",
                        principalColumn: "ValueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_ProductAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalSchema: "Catalog",
                        principalTable: "ProductAttributes",
                        principalColumn: "AttributeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Catalog",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValues_AttributeId",
                schema: "Catalog",
                table: "AttributeValues",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValues_AttributeId_DisplayOrder",
                schema: "Catalog",
                table: "AttributeValues",
                columns: new[] { "AttributeId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_AttributeName",
                schema: "Catalog",
                table: "ProductAttributes",
                column: "AttributeName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_CategoryId_IsFilterable",
                schema: "Catalog",
                table: "ProductAttributes",
                columns: new[] { "CategoryId", "IsFilterable" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_Attribute_Numeric",
                schema: "Catalog",
                table: "ProductAttributeValues",
                columns: new[] { "AttributeId", "NumericValue" },
                filter: "[NumericValue] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_Attribute_Value",
                schema: "Catalog",
                table: "ProductAttributeValues",
                columns: new[] { "AttributeId", "ValueId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_ValueId",
                schema: "Catalog",
                table: "ProductAttributeValues",
                column: "ValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAttributeValues",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "AttributeValues",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "ProductAttributes",
                schema: "Catalog");
        }
    }
}
