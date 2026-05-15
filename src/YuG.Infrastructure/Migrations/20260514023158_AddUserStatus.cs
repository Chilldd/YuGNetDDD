using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6887),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 51, DateTimeKind.Utc).AddTicks(5560));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6664),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 51, DateTimeKind.Utc).AddTicks(5337));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "User",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 760, DateTimeKind.Utc).AddTicks(4146),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 40, DateTimeKind.Utc).AddTicks(6779));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 758, DateTimeKind.Utc).AddTicks(4238),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 38, DateTimeKind.Utc).AddTicks(8055));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7426),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 75, DateTimeKind.Utc).AddTicks(6568));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7116),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 75, DateTimeKind.Utc).AddTicks(6342));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 795, DateTimeKind.Utc).AddTicks(6276),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 73, DateTimeKind.Utc).AddTicks(7664));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "User");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 51, DateTimeKind.Utc).AddTicks(5560),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6887));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 51, DateTimeKind.Utc).AddTicks(5337),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 771, DateTimeKind.Utc).AddTicks(6664));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 40, DateTimeKind.Utc).AddTicks(6779),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 760, DateTimeKind.Utc).AddTicks(4146));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 38, DateTimeKind.Utc).AddTicks(8055),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 758, DateTimeKind.Utc).AddTicks(4238));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 75, DateTimeKind.Utc).AddTicks(6568),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7426));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 75, DateTimeKind.Utc).AddTicks(6342),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 797, DateTimeKind.Utc).AddTicks(7116));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 13, 5, 53, 46, 73, DateTimeKind.Utc).AddTicks(7664),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 5, 14, 2, 31, 57, 795, DateTimeKind.Utc).AddTicks(6276));
        }
    }
}
