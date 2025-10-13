namespace Yippy.Emailing;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Enqueued email {Id} into the database")]
    public static partial void EmailEnqueued(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "An error occured in EmailingService execution loop")]
    public static partial void EmailServiceExecutionError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Processing {Count} emails")]
    public static partial void ProcessingEmails(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Could not acquire lock for email {EmailId}")]
    public static partial void CouldNotAcquireLock(this ILogger logger, Guid emailId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Error processing email {EmailId}")]
    public static partial void ErrorProcessingEmail(this ILogger logger, Exception ex, Guid emailId);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Successfully sent email {EmailId} to {RecipientCount} recipients")]
    public static partial void EmailSentSuccessfully(this ILogger logger, Guid emailId, int recipientCount);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Failed to send email {EmailId}")]
    public static partial void FailedToSendEmail(this ILogger logger, Exception ex, Guid emailId);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Warning,
        Message = "Email {EmailId} failed after {RetryCount} attempts: {Error}")]
    public static partial void EmailFailedAfterRetries(this ILogger logger, Guid emailId, int retryCount, string error);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "Email {EmailId} failed (attempt {RetryCount}), will retry at {NextRetryAt}: {Error}")]
    public static partial void EmailFailedWillRetry(this ILogger logger, Guid emailId, int retryCount, DateTime nextRetryAt, string error);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "An error occured during email cleanup")]
    public static partial void EmailCleanupError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "An error occured when resetting expired locks")]
    public static partial void ResetExpiredLocksError(this ILogger logger, Exception ex);
}
