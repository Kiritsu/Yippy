namespace Yippy.News;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "An exception occured when getting a Post")]
    public static partial void ExceptionGettingPost(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "An exception occured when creating a Post")]
    public static partial void ExceptionCreatingPost(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "[DeletePost] The post {Id} was not found")]
    public static partial void DeletePostNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "[DeletePost] Deleting the post {Id} failed ({Count} affected rows)")]
    public static partial void DeletePostFailed(this ILogger logger, Guid id, int count);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "An exception occured when deleting the post {Id}")]
    public static partial void ExceptionDeletingPost(this ILogger logger, Exception ex, Guid id);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "An exception occured when updating the post {Id}")]
    public static partial void ExceptionUpdatingPost(this ILogger logger, Exception ex, Guid id);
}
