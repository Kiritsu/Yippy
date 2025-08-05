using MassTransit;

namespace Yippy.Messaging.Default;

/// <summary>
/// Represents a generic implementation of a MassTransit consumer.
/// </summary>
/// <typeparam name="T">The type of message to consume.</typeparam>
/// <param name="messageConsumer">The instance of the user's consumer implementation.</param>
public class GenericMessageConsumer<T>(IMessageConsumer<T> messageConsumer) : IConsumer<T> where T : class, IMessage
{
    /// <summary>
    /// Consumes the message received by the broker and forwards it to the actual user's consumer.
    /// </summary>
    /// <param name="context">The instance of the message consume context.</param>
    public async Task Consume(ConsumeContext<T> context)
    {
        await messageConsumer.HandleAsync(context.Message);
    }
}
