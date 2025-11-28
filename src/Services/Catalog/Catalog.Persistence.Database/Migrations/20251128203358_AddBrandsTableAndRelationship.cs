using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandsTableAndRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                schema: "Catalog",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brands",
                schema: "Catalog",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                schema: "Catalog",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_IsActive",
                schema: "Catalog",
                table: "Brands",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                schema: "Catalog",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                schema: "Catalog",
                table: "Products",
                column: "BrandId",
                principalSchema: "Catalog",
                principalTable: "Brands",
                principalColumn: "BrandId",
                onDelete: ReferentialAction.SetNull);

            // Migración de datos: Extraer marcas únicas y crear registros en tabla Brands
            migrationBuilder.Sql(@"
                -- Insertar marcas únicas desde la columna Brand de Products
                INSERT INTO [Catalog].[Brands] ([Name], [Description], [LogoUrl], [IsActive])
                SELECT DISTINCT
                    [Brand],
                    NULL,
                    NULL,
                    1
                FROM [Catalog].[Products]
                WHERE [Brand] IS NOT NULL AND [Brand] <> ''
                ORDER BY [Brand];
            ");

            // Actualizar BrandId en Products basándose en el nombre de la marca
            migrationBuilder.Sql(@"
                -- Actualizar BrandId en Products
                UPDATE p
                SET p.[BrandId] = b.[BrandId]
                FROM [Catalog].[Products] p
                INNER JOIN [Catalog].[Brands] b ON p.[Brand] = b.[Name]
                WHERE p.[Brand] IS NOT NULL AND p.[Brand] <> '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Brands",
                schema: "Catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandId",
                schema: "Catalog",
                table: "Products");
        }
    }
}
