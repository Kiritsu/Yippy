namespace Yippy.Messaging;

/// <summary>
/// Defines a messaging service that sends the given messages to a messaging broker.
/// </summary>
public interface IMessagingService
{
    /// <summary>
    /// Enqueues a message that will be sent to the broker.
    /// </summary>
    /// <typeparam name="T">The type of message to be sent.</typeparam>
    /// <param name="message">The message to sent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default) where T : class, IMessage;
}
