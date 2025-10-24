using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Customer.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCustomerModel_Clean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "Customer",
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 10);

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Customer",
                table: "Clients",
                newName: "LastName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Customer",
                table: "Clients",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                schema: "Customer",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                schema: "Customer",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Customer",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                schema: "Customer",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneVerified",
                schema: "Customer",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                schema: "Customer",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NewsletterSubscribed",
                schema: "Customer",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCurrency",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "es");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmsNotificationsEnabled",
                schema: "Customer",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Customer",
                table: "Clients",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientAddresses",
                schema: "Customer",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddressName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDefaultShipping = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDefaultBilling = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAddresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_ClientAddresses_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "Customer",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                schema: "Customer",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UserId",
                schema: "Customer",
                table: "Clients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAddresses_ClientId_DefaultBilling",
                schema: "Customer",
                table: "ClientAddresses",
                columns: new[] { "ClientId", "IsDefaultBilling" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAddresses_ClientId_DefaultShipping",
                schema: "Customer",
                table: "ClientAddresses",
                columns: new[] { "ClientId", "IsDefaultShipping" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAddresses",
                schema: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_UserId",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsPhoneVerified",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NewsletterSubscribed",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredCurrency",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SmsNotificationsEnabled",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "Customer",
                table: "Clients",
                newName: "Name");

            migrationBuilder.InsertData(
                schema: "Customer",
                table: "Clients",
                columns: new[] { "ClientId", "Name" },
                values: new object[,]
                {
                    { 1, "Client 1" },
                    { 2, "Client 2" },
                    { 3, "Client 3" },
                    { 4, "Client 4" },
                    { 5, "Client 5" },
                    { 6, "Client 6" },
                    { 7, "Client 7" },
                    { 8, "Client 8" },
                    { 9, "Client 9" },
                    { 10, "Client 10" }
                });
        }
    }
}
