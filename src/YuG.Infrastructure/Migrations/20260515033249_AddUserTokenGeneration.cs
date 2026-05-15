using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTokenGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1706),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6887));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1480),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6664));

            migrationBuilder.AddColumn<int>(
                name: "Generation",
                table: "User",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 549, DateTimeKind.Utc).AddTicks(9773),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 760, DateTimeKind.Utc).AddTicks(4146));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 547, DateTimeKind.Utc).AddTicks(4909),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 758, DateTimeKind.Utc).AddTicks(4238));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1646),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7426));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1422),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7116));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 588, DateTimeKind.Utc).AddTicks(807),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 795, DateTimeKind.Utc).AddTicks(6276));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Generation",
                table: "User");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6887),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1706));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6664),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 562, DateTimeKind.Utc).AddTicks(1480));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 760, DateTimeKind.Utc).AddTicks(4146),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 549, DateTimeKind.Utc).AddTicks(9773));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 758, DateTimeKind.Utc).AddTicks(4238),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 547, DateTimeKind.Utc).AddTicks(4909));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7426),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1646));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7116),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 590, DateTimeKind.Utc).AddTicks(1422));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 795, DateTimeKind.Utc).AddTicks(6276),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 15, 3, 32, 48, 588, DateTimeKind.Utc).AddTicks(807));
        }
    }
}
