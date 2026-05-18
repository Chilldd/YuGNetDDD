using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiChatMessageToolFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 353, DateTimeKind.Utc).AddTicks(6388),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 829, DateTimeKind.Utc).AddTicks(8228));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 353, DateTimeKind.Utc).AddTicks(6168),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 829, DateTimeKind.Utc).AddTicks(8011));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 346, DateTimeKind.Utc).AddTicks(3062),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 823, DateTimeKind.Utc).AddTicks(2194));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 346, DateTimeKind.Utc).AddTicks(2572),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 823, DateTimeKind.Utc).AddTicks(1958));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 358, DateTimeKind.Utc).AddTicks(1865),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 834, DateTimeKind.Utc).AddTicks(4503));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 358, DateTimeKind.Utc).AddTicks(1671),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 834, DateTimeKind.Utc).AddTicks(4285));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 356, DateTimeKind.Utc).AddTicks(2403),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 832, DateTimeKind.Utc).AddTicks(5235));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AiChatSession",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 316, DateTimeKind.Utc).AddTicks(2050),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 791, DateTimeKind.Utc).AddTicks(4284));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AiChatSession",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 314, DateTimeKind.Utc).AddTicks(4707),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 789, DateTimeKind.Utc).AddTicks(6517));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AiChatMessage",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 344, DateTimeKind.Utc).AddTicks(6993),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 821, DateTimeKind.Utc).AddTicks(5157));

            migrationBuilder.AddColumn<string>(
                name: "ToolCallId",
                table: "AiChatMessage",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolCalls",
                table: "AiChatMessage",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolCallId",
                table: "AiChatMessage");

            migrationBuilder.DropColumn(
                name: "ToolCalls",
                table: "AiChatMessage");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 829, DateTimeKind.Utc).AddTicks(8228),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 353, DateTimeKind.Utc).AddTicks(6388));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 829, DateTimeKind.Utc).AddTicks(8011),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 353, DateTimeKind.Utc).AddTicks(6168));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 823, DateTimeKind.Utc).AddTicks(2194),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 346, DateTimeKind.Utc).AddTicks(3062));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 823, DateTimeKind.Utc).AddTicks(1958),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 346, DateTimeKind.Utc).AddTicks(2572));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 834, DateTimeKind.Utc).AddTicks(4503),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 358, DateTimeKind.Utc).AddTicks(1865));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 834, DateTimeKind.Utc).AddTicks(4285),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 358, DateTimeKind.Utc).AddTicks(1671));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 832, DateTimeKind.Utc).AddTicks(5235),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 356, DateTimeKind.Utc).AddTicks(2403));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AiChatSession",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 791, DateTimeKind.Utc).AddTicks(4284),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 316, DateTimeKind.Utc).AddTicks(2050));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AiChatSession",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 789, DateTimeKind.Utc).AddTicks(6517),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 314, DateTimeKind.Utc).AddTicks(4707));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AiChatMessage",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 17, 11, 1, 27, 821, DateTimeKind.Utc).AddTicks(5157),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 18, 5, 25, 5, 344, DateTimeKind.Utc).AddTicks(6993));
        }
    }
}
