using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customer.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicatedFieldsFromClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "Customer",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "Customer",
                table: "Clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "Customer",
                table: "Clients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                schema: "Customer",
                table: "Clients",
                column: "Email",
                unique: true);
        }
    }
}
