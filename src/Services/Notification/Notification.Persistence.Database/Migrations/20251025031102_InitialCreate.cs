using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Notification");

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                schema: "Notification",
                columns: table => new
                {
                    PreferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PushNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SMSNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderUpdates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Promotions = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Newsletter = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PriceAlerts = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StockAlerts = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.PreferenceId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "Notification",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MessageTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.TemplateId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                schema: "Notification",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                schema: "Notification",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExpiresAt",
                schema: "Notification",
                table: "Notifications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                schema: "Notification",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                schema: "Notification",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                schema: "Notification",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                schema: "Notification",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Type",
                schema: "Notification",
                table: "Notifications",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TemplateKey",
                schema: "Notification",
                table: "NotificationTemplates",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Type",
                schema: "Notification",
                table: "NotificationTemplates",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationPreferences",
                schema: "Notification");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "Notification");

            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "Notification");
        }
    }
}
