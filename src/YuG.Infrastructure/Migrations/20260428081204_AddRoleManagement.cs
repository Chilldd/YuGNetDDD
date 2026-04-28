using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(7164),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 627, DateTimeKind.Utc).AddTicks(4888));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(6952),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 625, DateTimeKind.Utc).AddTicks(2349));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(2185),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 658, DateTimeKind.Utc).AddTicks(7868));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(1978),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 658, DateTimeKind.Utc).AddTicks(7673));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 972, DateTimeKind.Utc).AddTicks(3387),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 657, DateTimeKind.Utc).AddTicks(1601));

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 936, DateTimeKind.Utc).AddTicks(4845)),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 938, DateTimeKind.Utc).AddTicks(2372))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleResource",
                columns: table => new
                {
                    ResourcesId = table.Column<long>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleResource", x => new { x.ResourcesId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RoleResource_Resource_ResourcesId",
                        column: x => x.ResourcesId,
                        principalTable: "Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleResource_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    RolesId = table.Column<long>(type: "INTEGER", nullable: false),
                    UsersId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UsersId",
                        column: x => x.UsersId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_Code",
                table: "Role",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleResource_RoleId",
                table: "RoleResource",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UsersId",
                table: "UserRole",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleResource");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 627, DateTimeKind.Utc).AddTicks(4888),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(7164));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 625, DateTimeKind.Utc).AddTicks(2349),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 948, DateTimeKind.Utc).AddTicks(6952));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 658, DateTimeKind.Utc).AddTicks(7868),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(2185));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 658, DateTimeKind.Utc).AddTicks(7673),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 974, DateTimeKind.Utc).AddTicks(1978));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshToken",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 7, 47, 37, 657, DateTimeKind.Utc).AddTicks(1601),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 12, 3, 972, DateTimeKind.Utc).AddTicks(3387));
        }
    }
}
