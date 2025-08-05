using System.Reflection;

namespace Yippy.Messaging.Default;

/// <summary>
/// Represents a generic subscribed message.
/// </summary>
/// <typeparam name="T">The type of the subscribed message.</typeparam>
public class GenericSubscribedMessage<T> : ISubscribedMessage
{
    /// <inheritdoc/>
    public Type Type => typeof(T);

    /// <inheritdoc/>
    public string Name => Type.GetCustomAttribute<MessageTypeAttribute>()?.Name ?? Type.Name;
}
