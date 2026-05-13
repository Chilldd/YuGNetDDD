using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleIsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 402, DateTimeKind.Utc).AddTicks(5667),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(7164));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 402, DateTimeKind.Utc).AddTicks(5459),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(6952));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 391, DateTimeKind.Utc).AddTicks(7365),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 938, DateTimeKind.Utc).AddTicks(2372));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 389, DateTimeKind.Utc).AddTicks(8881),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 936, DateTimeKind.Utc).AddTicks(4845));

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Role",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 426, DateTimeKind.Utc).AddTicks(1692),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(2185));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 426, DateTimeKind.Utc).AddTicks(1434),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(1978));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 424, DateTimeKind.Utc).AddTicks(1489),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 972, DateTimeKind.Utc).AddTicks(3387));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Role");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(7164),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 402, DateTimeKind.Utc).AddTicks(5667));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(6952),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 402, DateTimeKind.Utc).AddTicks(5459));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 938, DateTimeKind.Utc).AddTicks(2372),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 391, DateTimeKind.Utc).AddTicks(7365));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 936, DateTimeKind.Utc).AddTicks(4845),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 389, DateTimeKind.Utc).AddTicks(8881));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(2185),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 426, DateTimeKind.Utc).AddTicks(1692));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(1978),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 426, DateTimeKind.Utc).AddTicks(1434));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 972, DateTimeKind.Utc).AddTicks(3387),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 4, 57, 35, 424, DateTimeKind.Utc).AddTicks(1489));
        }
    }
}
