using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Yippy.Emailing.Data;

#nullable disable

namespace Yippy.Emailing.Migrations
{
    /// <inheritdoc />
    public partial class MapToJsonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ToRecipients",
                table: "QueuedEmails",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(List<EmailRecipient>),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<EmailRecipient>>(
                name: "ToRecipients",
                table: "QueuedEmails",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
