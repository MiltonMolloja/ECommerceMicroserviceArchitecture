using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordChangedAtToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "03b9e90c-0362-479b-a543-f569e4558c34");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangedAt",
                schema: "Identity",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "1e78d0d9-6f7e-409f-b9a4-dcb5c0e1442b", null, "Admin", "ADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1e78d0d9-6f7e-409f-b9a4-dcb5c0e1442b");

            migrationBuilder.DropColumn(
                name: "PasswordChangedAt",
                schema: "Identity",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "03b9e90c-0362-479b-a543-f569e4558c34", null, "Admin", "ADMIN" });
        }
    }
}
