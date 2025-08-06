using Microsoft.EntityFrameworkCore;

namespace Yippy.Emailing.Data;

public class EmailDbContext(DbContextOptions<EmailDbContext> options) : DbContext(options)
{
    public DbSet<QueuedEmail> QueuedEmails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QueuedEmail>(entity =>
        {
            entity.Property(e => e.FromName)
                .HasMaxLength(255);

            entity.Property(e => e.FromEmail)
                .HasMaxLength(255);

            entity.Property(e => e.Subject)
                .HasMaxLength(500);

            entity.Property(e => e.Body)
                .HasColumnType("text");

            entity.Property(e => e.ContentType)
                .HasMaxLength(50);

            entity.OwnsMany(e => e.ToRecipients, b =>
            {
                b.ToJson();
            });

            entity.Property(e => e.LockId)
                .HasMaxLength(36);

            entity.Property(e => e.LastErrorMessage)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_queued_emails_status");

            entity.HasIndex(e => new { e.Status, NextRetryAt = e.NextRetryAtUtc })
                .HasDatabaseName("ix_queued_emails_status_next_retry_at");

            entity.HasIndex(e => e.CreatedAtUtc)
                .HasDatabaseName("ix_queued_emails_created_at");

            entity.HasIndex(e => new { LockedUntil = e.LockedUntilUtc, e.LockId })
                .HasDatabaseName("ix_queued_emails_lock");

            entity.HasIndex(e => e.UpdatedAtUtc)
                .HasDatabaseName("ix_queued_emails_updated_at");
        });
    }
}