using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0bb5d877-15ea-4ce7-95bb-11742a14bcab");

            migrationBuilder.CreateTable(
                name: "UserAuditLogs",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBackupCodes",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBackupCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBackupCodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "03b9e90c-0362-479b-a543-f569e4558c34", null, "Admin", "ADMIN" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_Timestamp",
                schema: "Identity",
                table: "UserAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_UserId",
                schema: "Identity",
                table: "UserAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_UserId_Timestamp",
                schema: "Identity",
                table: "UserAuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserBackupCodes_UserId_IsUsed",
                schema: "Identity",
                table: "UserBackupCodes",
                columns: new[] { "UserId", "IsUsed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAuditLogs",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserBackupCodes",
                schema: "Identity");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "03b9e90c-0362-479b-a543-f569e4558c34");

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0bb5d877-15ea-4ce7-95bb-11742a14bcab", null, "Admin", "ADMIN" });
        }
    }
}
