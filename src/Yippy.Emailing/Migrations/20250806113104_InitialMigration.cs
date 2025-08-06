using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Yippy.Emailing.Data;

#nullable disable

namespace Yippy.Emailing.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueuedEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FromEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ToRecipients = table.Column<List<EmailRecipient>>(type: "jsonb", nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NextRetryAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedEmails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_queued_emails_created_at",
                table: "QueuedEmails",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "ix_queued_emails_lock",
                table: "QueuedEmails",
                columns: new[] { "LockedUntilUtc", "LockId" });

            migrationBuilder.CreateIndex(
                name: "ix_queued_emails_status",
                table: "QueuedEmails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_queued_emails_status_next_retry_at",
                table: "QueuedEmails",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_queued_emails_updated_at",
                table: "QueuedEmails",
                column: "UpdatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueuedEmails");
        }
    }
}
