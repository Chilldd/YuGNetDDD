using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 963, DateTimeKind.Utc).AddTicks(4751),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1706));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 963, DateTimeKind.Utc).AddTicks(4527),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1480));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 956, DateTimeKind.Utc).AddTicks(2468),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 549, DateTimeKind.Utc).AddTicks(9773));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 956, DateTimeKind.Utc).AddTicks(2228),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 547, DateTimeKind.Utc).AddTicks(4909));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 968, DateTimeKind.Utc).AddTicks(8000),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1646));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 968, DateTimeKind.Utc).AddTicks(7729),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1422));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 966, DateTimeKind.Utc).AddTicks(6151),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 588, DateTimeKind.Utc).AddTicks(807));

            migrationBuilder.CreateTable(
                name: "ChatSession",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: "新对话"),
                    LastActiveAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 921, DateTimeKind.Utc).AddTicks(5360)),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 923, DateTimeKind.Utc).AddTicks(7021))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    SequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenCount = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 954, DateTimeKind.Utc).AddTicks(5546)),
                    ChatSessionId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_ChatSession_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalTable: "ChatSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_ChatSessionId",
                table: "ChatMessage",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_SessionId",
                table: "ChatSession",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UserId",
                table: "ChatSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessage");

            migrationBuilder.DropTable(
                name: "ChatSession");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1706),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 963, DateTimeKind.Utc).AddTicks(4751));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1480),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 963, DateTimeKind.Utc).AddTicks(4527));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 549, DateTimeKind.Utc).AddTicks(9773),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 956, DateTimeKind.Utc).AddTicks(2468));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 547, DateTimeKind.Utc).AddTicks(4909),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 956, DateTimeKind.Utc).AddTicks(2228));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1646),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 968, DateTimeKind.Utc).AddTicks(8000));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1422),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 968, DateTimeKind.Utc).AddTicks(7729));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 588, DateTimeKind.Utc).AddTicks(807),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 27, 48, 966, DateTimeKind.Utc).AddTicks(6151));
        }
    }
}
