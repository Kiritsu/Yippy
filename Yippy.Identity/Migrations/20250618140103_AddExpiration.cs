using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yippy.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpiresInMinutes",
                table: "UserAccesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresInMinutes",
                table: "UserAccesses");
        }
    }
}
