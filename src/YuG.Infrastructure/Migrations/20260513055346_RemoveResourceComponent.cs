using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveResourceComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Component",
                table: "Resource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Component",
                table: "Resource",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
