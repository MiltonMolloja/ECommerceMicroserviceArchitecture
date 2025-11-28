using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingAndBillingAddressToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine1",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountry",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPostalCode",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BillingSameAsShipping",
                schema: "Order",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddressLine1",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddressLine2",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCountry",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingPostalCode",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingRecipientName",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingState",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingAddressLine1",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingCountry",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingPostalCode",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingSameAsShipping",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddressLine1",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddressLine2",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCountry",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPhone",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPostalCode",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingRecipientName",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingState",
                schema: "Order",
                table: "Orders");
        }
    }
}
