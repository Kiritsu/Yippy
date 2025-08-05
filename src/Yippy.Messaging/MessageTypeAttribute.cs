namespace Yippy.Messaging;

/// <summary>
/// Attribute that defines the message type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MessageTypeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public MessageTypeAttribute(string name) => Name = name;
}
