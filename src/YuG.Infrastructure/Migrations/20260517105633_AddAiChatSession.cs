using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiChatSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 272, DateTimeKind.Utc).AddTicks(5481),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1706));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 272, DateTimeKind.Utc).AddTicks(5274),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1480));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 265, DateTimeKind.Utc).AddTicks(6061),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 549, DateTimeKind.Utc).AddTicks(9773));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 265, DateTimeKind.Utc).AddTicks(5858),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 547, DateTimeKind.Utc).AddTicks(4909));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 276, DateTimeKind.Utc).AddTicks(9836),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1646));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 276, DateTimeKind.Utc).AddTicks(9439),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1422));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 275, DateTimeKind.Utc).AddTicks(890),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 588, DateTimeKind.Utc).AddTicks(807));

            migrationBuilder.CreateTable(
                name: "AiChatSession",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: "新对话"),
                    LastActiveAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 233, DateTimeKind.Utc).AddTicks(7072)),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 235, DateTimeKind.Utc).AddTicks(5606))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiChatMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    SequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenCount = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 264, DateTimeKind.Utc).AddTicks(961)),
                    AiChatSessionId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiChatMessage_AiChatSession_AiChatSessionId",
                        column: x => x.AiChatSessionId,
                        principalTable: "AiChatSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiChatMessage_AiChatSessionId",
                table: "AiChatMessage",
                column: "AiChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiChatSession_SessionId",
                table: "AiChatSession",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiChatSession_UserId",
                table: "AiChatSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiChatMessage");

            migrationBuilder.DropTable(
                name: "AiChatSession");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1706),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 272, DateTimeKind.Utc).AddTicks(5481));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1480),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 272, DateTimeKind.Utc).AddTicks(5274));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 549, DateTimeKind.Utc).AddTicks(9773),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 265, DateTimeKind.Utc).AddTicks(6061));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 547, DateTimeKind.Utc).AddTicks(4909),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 265, DateTimeKind.Utc).AddTicks(5858));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1646),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 276, DateTimeKind.Utc).AddTicks(9836));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1422),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 276, DateTimeKind.Utc).AddTicks(9439));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 588, DateTimeKind.Utc).AddTicks(807),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 10, 56, 33, 275, DateTimeKind.Utc).AddTicks(890));
        }
    }
}
