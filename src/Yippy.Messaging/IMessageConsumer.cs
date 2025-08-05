namespace Yippy.Messaging;

/// <summary>
/// Defines a generic message consumer.
/// </summary>
/// <typeparam name="T">The type of message to consume.</typeparam>
public interface IMessageConsumer<in T> where T : IMessage
{
    /// <summary>
    /// Consumes the message <typeparamref name="T"/> received.
    /// </summary>
    /// <param name="message">The instance of the message received.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}
