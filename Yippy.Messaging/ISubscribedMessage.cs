namespace Yippy.Messaging;

/// <summary>
/// Defines a message that is being consumed.
/// </summary>
public interface ISubscribedMessage
{
    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the name of the message for the exchange and the queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The name is the name given by the MessageType attribute on the message's class.
    /// </para>
    /// </remarks>
    string Name { get; }
}
