namespace Yippy.Messaging.Default;

/// <summary>
/// Represents a generic message publisher. It can be used by the user directly to send messages to the broker.
/// </summary>
/// <typeparam name="T">The type of message to send.</typeparam>
/// <param name="messagingService">The instance of the message service, sending the message to the broker.</param>
public class GenericMessagePublisher<T>(IMessagingService messagingService) : IMessagePublisher<T> where T : class, IMessage
{
    /// <inheritdoc/>
    public Task PublishAsync(T message, CancellationToken cancellationToken = default)
    {
        return messagingService.EnqueueAsync(message, cancellationToken);
    }
}
