using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCardHolderName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardHolderName",
                schema: "Payment",
                table: "PaymentDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardHolderName",
                schema: "Payment",
                table: "PaymentDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
