namespace Yippy.Messaging;

/// <summary>
/// Defines a generic message publisher. It can be used by the user directly to send messages to the broker.
/// </summary>
/// <typeparam name="T">The type of message to publish.</typeparam>
public interface IMessagePublisher<in T> where T : IMessage
{
    /// <summary>
    /// Publishes the message <typeparamref name="T"/> to the messaging service.
    /// </summary>
    /// <param name="message">The instance of the message to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(T message, CancellationToken cancellationToken = default);
}
