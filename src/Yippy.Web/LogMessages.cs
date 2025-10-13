namespace Yippy.Web;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Error getting authentication state")]
    public static partial void ErrorGettingAuthState(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to send email code for {Email}")]
    public static partial void FailedToSendEmailCode(this ILogger logger, Exception ex, string email);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Failed to validate access key")]
    public static partial void FailedToValidateAccessKey(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to refresh token")]
    public static partial void FailedToRefreshToken(this ILogger logger, Exception ex);
}
