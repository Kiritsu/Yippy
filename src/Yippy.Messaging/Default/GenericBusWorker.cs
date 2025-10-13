using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Channels;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Yippy.Messaging.Default;

/// <summary>
/// Represents a generic bus worker that processes message queues with timeout protection.
/// </summary>
public sealed class GenericBusWorker(IBusControl bus, ILogger<GenericBusWorker> logger)
    : BackgroundService, IMessagingService, IDisposable
{
    private readonly IBusControl _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    private readonly ILogger<GenericBusWorker> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentDictionary<Type, string> _messageTypeCache = new();
    private readonly Channel<MessageOperation> _messageChannel = Channel.CreateUnbounded<MessageOperation>();
    
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(180);
    private const string ExchangeUriFormat = "exchange:{0}";

    /// <summary>
    /// Starts the bus to process queued messages with timeout protection.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token for stopping the service.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.GenericBusWorkerStarted();
        
        try
        {
            await foreach (var messageOperation in _messageChannel.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessMessageOperation(messageOperation);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.GenericBusWorkerStopping();
        }
        catch (Exception ex)
        {
            _logger.GenericBusWorkerFatalError(ex);
            throw; // Re-throw to let the host handle the failure
        }
        finally
        {
            _logger.GenericBusWorkerStopped();
        }
    }

    /// <summary>
    /// Enqueues a message to be sent to the message broker.
    /// </summary>
    /// <typeparam name="T">The type of message to be sent.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the channel is closed.</exception>
    public async Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default) 
        where T : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageTypeName = GetOrCacheMessageTypeName<T>();
        var operation = CreateMessageOperation(message, messageTypeName);

        try
        {
            await _messageChannel.Writer.WriteAsync(operation, cancellationToken);
            _logger.MessageEnqueuedSuccessfully(typeof(T).Name);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.MessageEnqueueCancelled(typeof(T).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.FailedToEnqueueMessage(ex, typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Completes the message channel, preventing new messages from being enqueued.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _messageChannel.Writer.TryComplete();
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessMessageOperation(MessageOperation operation)
    {
        using var timeoutSource = new CancellationTokenSource(DefaultTimeout);
        
        try
        {
            await operation.ExecuteAsync(_bus, timeoutSource.Token);
            _logger.MessageOperationCompleted();
        }
        catch (OperationCanceledException) when (timeoutSource.Token.IsCancellationRequested)
        {
            _logger.MessageOperationTimedOut(DefaultTimeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.FailedToProcessMessageOperation(ex);
        }
    }

    private string GetOrCacheMessageTypeName<T>() where T : class, IMessage
    {
        var messageType = typeof(T);
        return _messageTypeCache.GetOrAdd(messageType, static type =>
        {
            var attribute = type.GetCustomAttribute<MessageTypeAttribute>();
            return attribute?.Name ?? type.Name;
        });
    }

    private static MessageOperation CreateMessageOperation<T>(T message, string messageTypeName) 
        where T : class, IMessage
    {
        return new MessageOperation(async (bus, cancellationToken) =>
        {
            var exchangeUri = new Uri(string.Format(ExchangeUriFormat, messageTypeName));
            var endpoint = await bus.GetSendEndpoint(exchangeUri);
            await endpoint.Send(message, cancellationToken);
        });
    }

    public new void Dispose()
    {
        _messageChannel.Writer.TryComplete();
        base.Dispose();
    }

    /// <summary>
    /// Represents an operation to send a message through the bus.
    /// </summary>
    private readonly record struct MessageOperation(Func<IBusControl, CancellationToken, Task> ExecuteAsync);
}