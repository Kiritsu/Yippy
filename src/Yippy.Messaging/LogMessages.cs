using Microsoft.Extensions.Logging;

namespace Yippy.Messaging;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Generic bus worker started")]
    public static partial void GenericBusWorkerStarted(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Generic bus worker is stopping due to cancellation request")]
    public static partial void GenericBusWorkerStopping(this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Fatal error occurred in generic bus worker")]
    public static partial void GenericBusWorkerFatalError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Generic bus worker stopped")]
    public static partial void GenericBusWorkerStopped(this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Message of type {MessageType} enqueued successfully")]
    public static partial void MessageEnqueuedSuccessfully(this ILogger logger, string messageType);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Message enqueue was cancelled for type {MessageType}")]
    public static partial void MessageEnqueueCancelled(this ILogger logger, string messageType);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Failed to enqueue message of type {MessageType}")]
    public static partial void FailedToEnqueueMessage(this ILogger logger, Exception ex, string messageType);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Message operation completed successfully")]
    public static partial void MessageOperationCompleted(this ILogger logger);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Message operation timed out after {Timeout} seconds")]
    public static partial void MessageOperationTimedOut(this ILogger logger, double timeout);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "Failed to process message operation")]
    public static partial void FailedToProcessMessageOperation(this ILogger logger, Exception ex);
}
