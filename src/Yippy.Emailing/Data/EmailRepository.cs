using Microsoft.EntityFrameworkCore;

namespace Yippy.Emailing.Data;

public interface IEmailRepository
{
    Task<Guid> EnqueueEmailAsync(EmailDetail email);
    Task<List<QueuedEmail>> GetPendingEmailsAsync(int batchSize = 10);
    Task<bool> LockEmailAsync(Guid emailId, string lockId, TimeSpan lockDuration);
    Task UpdateEmailStatusAsync(Guid emailId, EmailStatus status, string? errorMessage = null);
    Task IncrementRetryCountAsync(Guid emailId, DateTime nextRetryAt);
    Task<bool> ReleaseLockAsync(Guid emailId, string lockId);
    Task<int> CleanupOldEmailsAsync(TimeSpan olderThan);
    Task ResetExpiredLocksAsync();
    Task<QueuedEmail?> GetEmailByIdAsync(Guid emailId);
    Task<int> GetCountByStatusAsync(EmailStatus status);
}

public class EmailRepository(EmailDbContext context) : IEmailRepository
{
    public async Task<Guid> EnqueueEmailAsync(EmailDetail email)
    {
        var queuedEmail = new QueuedEmail
        {
            FromName = email.From.Name,
            FromEmail = email.From.Email,
            ToRecipients = email.To.Select(x => new EmailRecipient 
            { 
                Name = x.Name, 
                Email = x.Email 
            }).ToList(),
            Subject = email.Subject,
            Body = email.Body,
            ContentType = email.ContentType,
            Status = EmailStatus.Pending
        };

        context.QueuedEmails.Add(queuedEmail);
        await context.SaveChangesAsync();
        
        return queuedEmail.Id;
    }

    public async Task<List<QueuedEmail>> GetPendingEmailsAsync(int batchSize = 10)
    {
        var now = DateTime.UtcNow;
        
        return await context.QueuedEmails
            .Where(e => e.Status == EmailStatus.Pending && 
                       (e.NextRetryAtUtc == null || e.NextRetryAtUtc <= now) &&
                       (e.LockedUntilUtc == null || e.LockedUntilUtc <= now))
            .OrderBy(e => e.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<bool> LockEmailAsync(Guid emailId, string lockId, TimeSpan lockDuration)
    {
        var now = DateTime.UtcNow;
        var lockUntil = now.Add(lockDuration);
        
        var rowsAffected = await context.QueuedEmails
            .Where(e => e.Id == emailId && 
                       (e.LockedUntilUtc == null || e.LockedUntilUtc <= now) &&
                       e.Status == EmailStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.LockId, lockId)
                .SetProperty(e => e.LockedUntilUtc, lockUntil)
                .SetProperty(e => e.Status, EmailStatus.Processing)
                .SetProperty(e => e.UpdatedAtUtc, now));

        return rowsAffected > 0;
    }

    public async Task UpdateEmailStatusAsync(Guid emailId, EmailStatus status, string? errorMessage = null)
    {
        var now = DateTime.UtcNow;
        
        await context.QueuedEmails
            .Where(e => e.Id == emailId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, status)
                .SetProperty(e => e.ProcessedAtUtc, now)
                .SetProperty(e => e.LastErrorMessage, errorMessage)
                .SetProperty(e => e.LockId, (string?)null)
                .SetProperty(e => e.LockedUntilUtc, (DateTime?)null)
                .SetProperty(e => e.UpdatedAtUtc, now));
    }

    public async Task IncrementRetryCountAsync(Guid emailId, DateTime nextRetryAt)
    {
        var now = DateTime.UtcNow;
        
        await context.QueuedEmails
            .Where(e => e.Id == emailId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.RetryCount, e => e.RetryCount + 1)
                .SetProperty(e => e.NextRetryAtUtc, nextRetryAt)
                .SetProperty(e => e.Status, EmailStatus.Pending)
                .SetProperty(e => e.LockId, (string?)null)
                .SetProperty(e => e.LockedUntilUtc, (DateTime?)null)
                .SetProperty(e => e.UpdatedAtUtc, now));
    }

    public async Task<bool> ReleaseLockAsync(Guid emailId, string lockId)
    {
        var now = DateTime.UtcNow;
        
        var rowsAffected = await context.QueuedEmails
            .Where(e => e.Id == emailId && e.LockId == lockId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.LockId, (string?)null)
                .SetProperty(e => e.LockedUntilUtc, (DateTime?)null)
                .SetProperty(e => e.Status, EmailStatus.Pending)
                .SetProperty(e => e.UpdatedAtUtc, now));

        return rowsAffected > 0;
    }

    public async Task<int> CleanupOldEmailsAsync(TimeSpan olderThan)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
        
        var deletedCount = await context.QueuedEmails
            .Where(e => e.CreatedAtUtc < cutoffDate && 
                       (e.Status == EmailStatus.Sent || e.Status == EmailStatus.Failed))
            .ExecuteDeleteAsync();

        return deletedCount;
    }

    public async Task ResetExpiredLocksAsync()
    {
        var now = DateTime.UtcNow;
        
        await context.QueuedEmails
            .Where(e => e.LockedUntilUtc < now && e.Status == EmailStatus.Processing)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.LockId, (string?)null)
                .SetProperty(e => e.LockedUntilUtc, (DateTime?)null)
                .SetProperty(e => e.Status, EmailStatus.Pending)
                .SetProperty(e => e.UpdatedAtUtc, now));
    }

    public async Task<QueuedEmail?> GetEmailByIdAsync(Guid emailId)
    {
        return await context.QueuedEmails.FindAsync(emailId);
    }

    public async Task<int> GetCountByStatusAsync(EmailStatus status)
    {
        return await context.QueuedEmails.CountAsync(e => e.Status == status);
    }

    public async Task<int> MarkExpiredEmailsAsync(TimeSpan expireAfter)
    {
        var expireDate = DateTime.UtcNow.Subtract(expireAfter);
        
        var expiredCount = await context.QueuedEmails
            .Where(e => e.CreatedAtUtc < expireDate && 
                       e.Status == EmailStatus.Pending &&
                       e.RetryCount >= 3)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, EmailStatus.Expired)
                .SetProperty(e => e.UpdatedAtUtc, DateTime.UtcNow));

        return expiredCount;
    }

    public async Task BulkUpdateStatusAsync(List<Guid> emailIds, EmailStatus status)
    {
        var now = DateTime.UtcNow;
        
        await context.QueuedEmails
            .Where(e => emailIds.Contains(e.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, status)
                .SetProperty(e => e.UpdatedAtUtc, now));
    }
}