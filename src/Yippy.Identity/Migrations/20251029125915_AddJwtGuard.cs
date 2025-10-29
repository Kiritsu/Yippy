using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yippy.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddJwtGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JwtGuards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHashSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JwtGuards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JwtGuards_TokenExpiresAtUtc",
                table: "JwtGuards",
                column: "TokenExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JwtGuards_TokenHashSha256",
                table: "JwtGuards",
                column: "TokenHashSha256",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JwtGuards");
        }
    }
}
