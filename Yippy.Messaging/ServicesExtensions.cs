using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Yippy.Messaging.Default;

namespace Yippy.Messaging;

/// <summary>
/// Represents the extension methods that adds publish/consume pattern with a messaging broker.
/// </summary>
public static class ServicesExtensions
{
    /// <summary>
    /// Adds a message publisher of a specific message type.
    /// </summary>
    /// <typeparam name="T">The type of message that is being sent through this publisher.</typeparam>
    /// <param name="this">The instance of the service collection.</param>
    public static IServiceCollection AddMessagePublisher<T>(this IServiceCollection @this) where T : class, IMessage
    {
        @this.TryAddSingleton<IMessagePublisher<T>, GenericMessagePublisher<T>>();
        return @this;
    }

    /// <summary>
    /// Adds a consumer of a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that is being sent through this publisher.</typeparam>
    /// <typeparam name="TConsumer">The implementation type of the consumer.</typeparam>
    /// <param name="this">The instance of the service collection.</param>
    public static IServiceCollection AddMessageConsumer<TMessage, TConsumer>(this IServiceCollection @this)
        where TMessage : class, IMessage
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        @this.AddSingleton<ISubscribedMessage, GenericSubscribedMessage<TMessage>>();
        @this.AddSingleton<IMessageConsumer<TMessage>, TConsumer>();
        return @this;
    }

    /// <summary>
    /// Adds a rabbit mq messaging service implementation.
    /// </summary>
    /// <param name="this">The instance of the service collection.</param>
    public static void AddRabbitMqMessagingService(this IServiceCollection @this)
    {
        // adds the bus worker
        @this.AddSingleton<GenericBusWorker>();
        @this.AddSingleton<IMessagingService>(x => x.GetRequiredService<GenericBusWorker>());
        @this.AddHostedService(x => x.GetRequiredService<GenericBusWorker>());

        // adds the generic consumers
        @this.AddSingleton(typeof(GenericMessageConsumer<>));

        @this.AddMassTransit(x => x.UsingRabbitMq((ctx, cfg) =>
        {
            var options = ctx.GetRequiredService<IOptions<MessageBrokerOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.VirtualHost))
            {
                // configures the rabbitmq connection credentials and endpoint
                cfg.Host(options.Host, configure =>
                {
                    configure.Username(options.Username);
                    configure.Password(options.Password);
                });
            }
            else
            {
                // configures the rabbitmq connection credentials and endpoint on the specific virtual host
                cfg.Host(options.Host, options.VirtualHost, configure =>
                {
                    configure.Username(options.Username);
                    configure.Password(options.Password);
                });
            }

            // registers the different messages to subscribe to
            var messageTypes = ctx
                .GetServices<ISubscribedMessage>()
                .ToList();
            
            foreach (var genericMethod in messageTypes
                         .Select(messageType => GenericConfigure.MakeGenericMethod(messageType.Type)))
            {
                genericMethod.Invoke(null, [cfg]);
            }

            // configures the different messages to subscribe to
            cfg.ReceiveEndpoint(options.QueueName, configure =>
            {
                if (options.EnableRetry)
                {
                    configure.UseMessageRetry(r => 
                        r.Incremental(
                            options.MaxRetry, 
                            options.InitialRetryDelay, 
                            options.IncrementalDelay));
                }

                configure.Durable = true;
                configure.BindQueue = true;

                foreach (var messageType in messageTypes)
                {
                    // create a binding between the current queue and the message type
                    configure.Bind(messageType.Name);

                    // gives MassTransit its consumer that will forward the message to our actual consumer
                    var consumerType = typeof(GenericMessageConsumer<>).MakeGenericType(messageType.Type);
                    configure.Consumer(consumerType, _ =>
                    {
                        var consumerInstance = ctx.GetRequiredService(consumerType);
                        return consumerInstance;
                    });
                }
            });
        }));
    }

    private static readonly MethodInfo GenericConfigure =
        typeof(ServicesExtensions).GetMethod(nameof(ConfigureMessage), BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>
    /// Configures a message typology on the broker.
    /// </summary>
    /// <typeparam name="T">The type of message to configure.</typeparam>
    /// <param name="cfg">The instance of the bus configurator.</param>
    private static void ConfigureMessage<T>(IBusFactoryConfigurator cfg)
        where T : class, IMessage
    {
        var messageType = typeof(T).GetCustomAttribute<MessageTypeAttribute>();
        var messageTypeName = messageType?.Name ?? typeof(T).Name;
        cfg.Message<T>(s => s.SetEntityName(messageTypeName));
    }
}
