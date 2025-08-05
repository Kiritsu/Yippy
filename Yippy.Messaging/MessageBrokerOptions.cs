namespace Yippy.Messaging;

/// <summary>
/// Defines the different message broker connection options.
/// </summary>
public class MessageBrokerOptions
{
    /// <summary>
    /// Gets the configuration option section name.
    /// </summary>
    public const string Name = "MessageBroker";

    /// <summary>
    /// Gets or sets the host of the rabbitmq instance.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username of the rabbitmq account.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password of the rabbitmq account.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the queue. Must be unique to the service name.
    /// </summary>
    public string QueueName { get; set; } = null!;

    /// <summary>
    /// Gets whether message retry is enabled or not.
    /// </summary>
    public bool EnableRetry { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum amount of retry.
    /// </summary>
    public int MaxRetry { get; set; } = 5;

    /// <summary>
    /// Gets or sets the retry delay.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the retry incremental delay.
    /// </summary>
    public TimeSpan IncrementalDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the facultative virtual host to use.
    /// </summary>
    public string? VirtualHost { get; set; }
}
