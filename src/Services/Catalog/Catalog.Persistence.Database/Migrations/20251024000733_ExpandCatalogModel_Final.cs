using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCatalogModel_Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================
            // Step 1: Delete existing seed data using SQL
            // =============================================
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Stocks_Products_ProductId')
                BEGIN
                    DELETE FROM [Catalog].[Stocks];
                END
                DELETE FROM [Catalog].[Products];
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Products_ProductId",
                schema: "Catalog",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stocks",
                schema: "Catalog",
                table: "Stocks");

            // Seed data already deleted with SQL above - skip all DeleteData operations

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 43);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 57);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 58);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 59);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 64);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 65);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 66);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 67);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 68);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 69);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 70);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 71);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 72);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 73);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 74);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 75);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 76);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 77);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 78);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 79);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 80);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 81);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 82);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 83);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 84);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 85);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 86);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 87);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 88);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 89);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 90);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 91);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 92);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 93);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 94);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 95);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 96);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 97);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 98);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 99);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Stocks",
                keyColumn: "ProductInStockId",
                keyValue: 100);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 43);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 57);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 58);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 59);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 64);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 65);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 66);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 67);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 68);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 69);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 70);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 71);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 72);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 73);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 74);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 75);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 76);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 77);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 78);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 79);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 80);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 81);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 82);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 83);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 84);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 85);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 86);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 87);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 88);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 89);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 90);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 91);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 92);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 93);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 94);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 95);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 96);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 97);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 98);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 99);

            migrationBuilder.DeleteData(
                schema: "Catalog",
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 100);

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Stocks",
                schema: "Catalog",
                newName: "ProductInStock",
                newSchema: "Catalog");

            migrationBuilder.RenameIndex(
                name: "IX_Stocks_ProductId",
                schema: "Catalog",
                table: "ProductInStock",
                newName: "IX_ProductInStock_ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Catalog",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEnglish",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionSpanish",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                schema: "Catalog",
                table: "Products",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Images",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Catalog",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                schema: "Catalog",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglish",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameSpanish",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                schema: "Catalog",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                schema: "Catalog",
                table: "Products",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                schema: "Catalog",
                table: "ProductInStock",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MaxStock",
                schema: "Catalog",
                table: "ProductInStock",
                type: "int",
                nullable: false,
                defaultValue: 1000);

            migrationBuilder.AddColumn<int>(
                name: "MinStock",
                schema: "Catalog",
                table: "ProductInStock",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductInStock",
                schema: "Catalog",
                table: "ProductInStock",
                column: "ProductInStockId");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Catalog",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameSpanish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionSpanish = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEnglish = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalSchema: "Catalog",
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                schema: "Catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => new { x.ProductId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ProductCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Catalog",
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Catalog",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand",
                schema: "Catalog",
                table: "Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                schema: "Catalog",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_IsFeatured",
                schema: "Catalog",
                table: "Products",
                columns: new[] { "IsActive", "IsFeatured" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured",
                schema: "Catalog",
                table: "Products",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                schema: "Catalog",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                schema: "Catalog",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaxStock_Valid",
                schema: "Catalog",
                table: "ProductInStock",
                sql: "[MaxStock] > [MinStock]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MinStock_Positive",
                schema: "Catalog",
                table: "ProductInStock",
                sql: "[MinStock] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stock_Positive",
                schema: "Catalog",
                table: "ProductInStock",
                sql: "[Stock] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                schema: "Catalog",
                table: "Categories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                schema: "Catalog",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                schema: "Catalog",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                schema: "Catalog",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryId",
                schema: "Catalog",
                table: "ProductCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_IsPrimary",
                schema: "Catalog",
                table: "ProductCategories",
                column: "IsPrimary");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInStock_Products_ProductId",
                schema: "Catalog",
                table: "ProductInStock",
                column: "ProductId",
                principalSchema: "Catalog",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInStock_Products_ProductId",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.DropTable(
                name: "ProductCategories",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_Brand",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_IsFeatured",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsFeatured",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductInStock",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MaxStock_Valid",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MinStock_Positive",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stock_Positive",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.DropColumn(
                name: "Brand",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionEnglish",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionSpanish",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Images",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NameEnglish",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NameSpanish",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SKU",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaxStock",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.DropColumn(
                name: "MinStock",
                schema: "Catalog",
                table: "ProductInStock");

            migrationBuilder.RenameTable(
                name: "ProductInStock",
                schema: "Catalog",
                newName: "Stocks",
                newSchema: "Catalog");

            migrationBuilder.RenameIndex(
                name: "IX_ProductInStock_ProductId",
                schema: "Catalog",
                table: "Stocks",
                newName: "IX_Stocks_ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                schema: "Catalog",
                table: "Stocks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stocks",
                schema: "Catalog",
                table: "Stocks",
                column: "ProductInStockId");

            migrationBuilder.InsertData(
                schema: "Catalog",
                table: "Products",
                columns: new[] { "ProductId", "Description", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Description for product 1", "Product 1", 739m },
                    { 2, "Description for product 2", "Product 2", 166m },
                    { 3, "Description for product 3", "Product 3", 251m },
                    { 4, "Description for product 4", "Product 4", 903m },
                    { 5, "Description for product 5", "Product 5", 523m },
                    { 6, "Description for product 6", "Product 6", 141m },
                    { 7, "Description for product 7", "Product 7", 928m },
                    { 8, "Description for product 8", "Product 8", 668m },
                    { 9, "Description for product 9", "Product 9", 747m },
                    { 10, "Description for product 10", "Product 10", 588m },
                    { 11, "Description for product 11", "Product 11", 775m },
                    { 12, "Description for product 12", "Product 12", 859m },
                    { 13, "Description for product 13", "Product 13", 440m },
                    { 14, "Description for product 14", "Product 14", 967m },
                    { 15, "Description for product 15", "Product 15", 679m },
                    { 16, "Description for product 16", "Product 16", 665m },
                    { 17, "Description for product 17", "Product 17", 507m },
                    { 18, "Description for product 18", "Product 18", 280m },
                    { 19, "Description for product 19", "Product 19", 358m },
                    { 20, "Description for product 20", "Product 20", 544m },
                    { 21, "Description for product 21", "Product 21", 194m },
                    { 22, "Description for product 22", "Product 22", 630m },
                    { 23, "Description for product 23", "Product 23", 108m },
                    { 24, "Description for product 24", "Product 24", 191m },
                    { 25, "Description for product 25", "Product 25", 405m },
                    { 26, "Description for product 26", "Product 26", 477m },
                    { 27, "Description for product 27", "Product 27", 913m },
                    { 28, "Description for product 28", "Product 28", 124m },
                    { 29, "Description for product 29", "Product 29", 306m },
                    { 30, "Description for product 30", "Product 30", 636m },
                    { 31, "Description for product 31", "Product 31", 928m },
                    { 32, "Description for product 32", "Product 32", 886m },
                    { 33, "Description for product 33", "Product 33", 770m },
                    { 34, "Description for product 34", "Product 34", 366m },
                    { 35, "Description for product 35", "Product 35", 738m },
                    { 36, "Description for product 36", "Product 36", 327m },
                    { 37, "Description for product 37", "Product 37", 216m },
                    { 38, "Description for product 38", "Product 38", 887m },
                    { 39, "Description for product 39", "Product 39", 735m },
                    { 40, "Description for product 40", "Product 40", 450m },
                    { 41, "Description for product 41", "Product 41", 749m },
                    { 42, "Description for product 42", "Product 42", 475m },
                    { 43, "Description for product 43", "Product 43", 487m },
                    { 44, "Description for product 44", "Product 44", 689m },
                    { 45, "Description for product 45", "Product 45", 259m },
                    { 46, "Description for product 46", "Product 46", 153m },
                    { 47, "Description for product 47", "Product 47", 573m },
                    { 48, "Description for product 48", "Product 48", 264m },
                    { 49, "Description for product 49", "Product 49", 732m },
                    { 50, "Description for product 50", "Product 50", 801m },
                    { 51, "Description for product 51", "Product 51", 482m },
                    { 52, "Description for product 52", "Product 52", 548m },
                    { 53, "Description for product 53", "Product 53", 544m },
                    { 54, "Description for product 54", "Product 54", 912m },
                    { 55, "Description for product 55", "Product 55", 835m },
                    { 56, "Description for product 56", "Product 56", 208m },
                    { 57, "Description for product 57", "Product 57", 158m },
                    { 58, "Description for product 58", "Product 58", 159m },
                    { 59, "Description for product 59", "Product 59", 598m },
                    { 60, "Description for product 60", "Product 60", 145m },
                    { 61, "Description for product 61", "Product 61", 227m },
                    { 62, "Description for product 62", "Product 62", 903m },
                    { 63, "Description for product 63", "Product 63", 462m },
                    { 64, "Description for product 64", "Product 64", 211m },
                    { 65, "Description for product 65", "Product 65", 660m },
                    { 66, "Description for product 66", "Product 66", 888m },
                    { 67, "Description for product 67", "Product 67", 188m },
                    { 68, "Description for product 68", "Product 68", 174m },
                    { 69, "Description for product 69", "Product 69", 329m },
                    { 70, "Description for product 70", "Product 70", 451m },
                    { 71, "Description for product 71", "Product 71", 549m },
                    { 72, "Description for product 72", "Product 72", 620m },
                    { 73, "Description for product 73", "Product 73", 545m },
                    { 74, "Description for product 74", "Product 74", 908m },
                    { 75, "Description for product 75", "Product 75", 795m },
                    { 76, "Description for product 76", "Product 76", 719m },
                    { 77, "Description for product 77", "Product 77", 243m },
                    { 78, "Description for product 78", "Product 78", 418m },
                    { 79, "Description for product 79", "Product 79", 932m },
                    { 80, "Description for product 80", "Product 80", 352m },
                    { 81, "Description for product 81", "Product 81", 904m },
                    { 82, "Description for product 82", "Product 82", 748m },
                    { 83, "Description for product 83", "Product 83", 392m },
                    { 84, "Description for product 84", "Product 84", 504m },
                    { 85, "Description for product 85", "Product 85", 254m },
                    { 86, "Description for product 86", "Product 86", 479m },
                    { 87, "Description for product 87", "Product 87", 441m },
                    { 88, "Description for product 88", "Product 88", 857m },
                    { 89, "Description for product 89", "Product 89", 530m },
                    { 90, "Description for product 90", "Product 90", 629m },
                    { 91, "Description for product 91", "Product 91", 269m },
                    { 92, "Description for product 92", "Product 92", 156m },
                    { 93, "Description for product 93", "Product 93", 389m },
                    { 94, "Description for product 94", "Product 94", 689m },
                    { 95, "Description for product 95", "Product 95", 322m },
                    { 96, "Description for product 96", "Product 96", 846m },
                    { 97, "Description for product 97", "Product 97", 113m },
                    { 98, "Description for product 98", "Product 98", 375m },
                    { 99, "Description for product 99", "Product 99", 129m },
                    { 100, "Description for product 100", "Product 100", 370m }
                });

            migrationBuilder.InsertData(
                schema: "Catalog",
                table: "Stocks",
                columns: new[] { "ProductInStockId", "ProductId", "Stock" },
                values: new object[,]
                {
                    { 1, 1, 17 },
                    { 2, 2, 8 },
                    { 3, 3, 18 },
                    { 4, 4, 10 },
                    { 5, 5, 13 },
                    { 6, 6, 16 },
                    { 7, 7, 11 },
                    { 8, 8, 4 },
                    { 9, 9, 3 },
                    { 10, 10, 13 },
                    { 11, 11, 5 },
                    { 12, 12, 9 },
                    { 13, 13, 9 },
                    { 14, 14, 16 },
                    { 15, 15, 0 },
                    { 16, 16, 6 },
                    { 17, 17, 9 },
                    { 18, 18, 5 },
                    { 19, 19, 0 },
                    { 20, 20, 8 },
                    { 21, 21, 14 },
                    { 22, 22, 19 },
                    { 23, 23, 19 },
                    { 24, 24, 6 },
                    { 25, 25, 1 },
                    { 26, 26, 13 },
                    { 27, 27, 10 },
                    { 28, 28, 4 },
                    { 29, 29, 10 },
                    { 30, 30, 16 },
                    { 31, 31, 18 },
                    { 32, 32, 3 },
                    { 33, 33, 6 },
                    { 34, 34, 16 },
                    { 35, 35, 5 },
                    { 36, 36, 16 },
                    { 37, 37, 16 },
                    { 38, 38, 2 },
                    { 39, 39, 13 },
                    { 40, 40, 4 },
                    { 41, 41, 6 },
                    { 42, 42, 2 },
                    { 43, 43, 0 },
                    { 44, 44, 2 },
                    { 45, 45, 2 },
                    { 46, 46, 7 },
                    { 47, 47, 12 },
                    { 48, 48, 18 },
                    { 49, 49, 18 },
                    { 50, 50, 10 },
                    { 51, 51, 5 },
                    { 52, 52, 15 },
                    { 53, 53, 15 },
                    { 54, 54, 6 },
                    { 55, 55, 17 },
                    { 56, 56, 18 },
                    { 57, 57, 9 },
                    { 58, 58, 11 },
                    { 59, 59, 8 },
                    { 60, 60, 0 },
                    { 61, 61, 5 },
                    { 62, 62, 7 },
                    { 63, 63, 13 },
                    { 64, 64, 7 },
                    { 65, 65, 14 },
                    { 66, 66, 2 },
                    { 67, 67, 2 },
                    { 68, 68, 13 },
                    { 69, 69, 11 },
                    { 70, 70, 4 },
                    { 71, 71, 10 },
                    { 72, 72, 7 },
                    { 73, 73, 11 },
                    { 74, 74, 16 },
                    { 75, 75, 2 },
                    { 76, 76, 12 },
                    { 77, 77, 18 },
                    { 78, 78, 17 },
                    { 79, 79, 3 },
                    { 80, 80, 14 },
                    { 81, 81, 0 },
                    { 82, 82, 12 },
                    { 83, 83, 5 },
                    { 84, 84, 0 },
                    { 85, 85, 10 },
                    { 86, 86, 3 },
                    { 87, 87, 7 },
                    { 88, 88, 0 },
                    { 89, 89, 19 },
                    { 90, 90, 7 },
                    { 91, 91, 7 },
                    { 92, 92, 4 },
                    { 93, 93, 13 },
                    { 94, 94, 13 },
                    { 95, 95, 19 },
                    { 96, 96, 18 },
                    { 97, 97, 8 },
                    { 98, 98, 13 },
                    { 99, 99, 7 },
                    { 100, 100, 19 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Products_ProductId",
                schema: "Catalog",
                table: "Stocks",
                column: "ProductId",
                principalSchema: "Catalog",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
