namespace Yippy.Identity;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Token generated: {Token}")]
    public static partial void TokenGenerated(this ILogger logger, Guid token);
    
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "An exception occured when creating a UserAccess")]
    public static partial void ExceptionCreatingUserAccess(this ILogger logger, Exception ex);
}
