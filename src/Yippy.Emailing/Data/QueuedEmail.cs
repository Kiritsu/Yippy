using System.Text.Json.Serialization;

namespace Yippy.Emailing.Data;

public class QueuedEmail
{
    public Guid Id { get; set; }
    
    public required string FromName { get; set; }
    
    public required string FromEmail { get; set; }
    
    public required List<EmailRecipient> ToRecipients { get; set; }
    
    public required string Subject { get; set; }
    
    public required string Body { get; set; }
    
    public required string ContentType { get; set; } = "html";
    
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    
    public int RetryCount { get; set; } = 0;
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAtUtc { get; set; }
    
    public DateTime? LockedUntilUtc { get; set; }
    
    public string? LockId { get; set; }
    
    public string? LastErrorMessage { get; set; }
    
    public DateTime? NextRetryAtUtc { get; set; }
    
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum EmailStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3,
    Expired = 4
}

public class EmailRecipient
{
    [JsonPropertyName("name")]
    public required string Name { get; set; } 
    
    [JsonPropertyName("email")]
    public required string Email { get; set; }
}