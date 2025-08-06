using System.Diagnostics.CodeAnalysis;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Yippy.Emailing.Data;

namespace Yippy.Emailing;

[SuppressMessage("ReSharper", "AsyncVoidLambda", 
    Justification = "The methods that are called are safe and won't throw any exception")]
public sealed class EmailingService : BackgroundService
{
    private readonly ILogger<EmailingService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<EmailOptions> _options;
    
    private readonly Timer _cleanupTimer;
    private readonly Timer _lockResetTimer;

    public EmailingService(
        IServiceProvider serviceProvider,
        IOptions<EmailOptions> options, 
        ILogger<EmailingService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
        
        _cleanupTimer = new Timer(async _ => await PerformCleanup(), null, 
            TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        // Resets expired locks when the service starts
        _lockResetTimer = new Timer(async _ => await ResetExpiredLocks(), null,
            TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailBatch(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured in EmailingService execution loop");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessEmailBatch(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
        
        var emails = await repository.GetPendingEmailsAsync(batchSize: 10);
        if (emails.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Processing {Count} emails", emails.Count);

        var lockId = Guid.NewGuid().ToString();
        var lockDuration = TimeSpan.FromMinutes(5);

        foreach (var email in emails.TakeWhile(_ => !cancellationToken.IsCancellationRequested))
        {
            var lockAcquired = await repository.LockEmailAsync(email.Id, lockId, lockDuration);
            if (!lockAcquired)
            {
                _logger.LogWarning("Could not acquire lock for email {EmailId}", email.Id);
                continue;
            }

            try
            {
                await ProcessSingleEmail(email, repository, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email {EmailId}", email.Id);
                await HandleEmailError(email, repository, ex.Message);
            }
        }
    }

    private async Task ProcessSingleEmail(QueuedEmail queuedEmail, IEmailRepository repository, CancellationToken cancellationToken)
    {
        using var smtpClient = new SmtpClient();
        
        try
        {
            await smtpClient.ConnectAsync(_options.Value.Hostname, _options.Value.Port, _options.Value.UseSsl, cancellationToken);
            await smtpClient.AuthenticateAsync(_options.Value.Username, _options.Value.Password, cancellationToken);

            var message = new MimeMessage
            {
                From = { new MailboxAddress(queuedEmail.FromName, queuedEmail.FromEmail) },
                Subject = queuedEmail.Subject,
                Body = new TextPart(queuedEmail.ContentType)
                {
                    Text = queuedEmail.Body
                }
            };

            foreach (var recipient in queuedEmail.ToRecipients)
            {
                message.To.Add(new MailboxAddress(recipient.Name, recipient.Email));
            }

            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            await repository.UpdateEmailStatusAsync(queuedEmail.Id, EmailStatus.Sent);
            
            _logger.LogInformation("Successfully sent email {EmailId} to {RecipientCount} recipients", 
                queuedEmail.Id, queuedEmail.ToRecipients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email {EmailId}", queuedEmail.Id);
            throw;
        }
        finally
        {
            if (smtpClient.IsConnected)
            {
                await smtpClient.DisconnectAsync(true, cancellationToken);
            }
        }
    }

    private async Task HandleEmailError(QueuedEmail queuedEmail, IEmailRepository repository, string errorMessage)
    {
        const int maxRetries = 3;
        
        if (queuedEmail.RetryCount >= maxRetries)
        {
            await repository.UpdateEmailStatusAsync(queuedEmail.Id, EmailStatus.Failed, errorMessage);
            _logger.LogWarning("Email {EmailId} failed after {RetryCount} attempts: {Error}", 
                queuedEmail.Id, queuedEmail.RetryCount, errorMessage);
        }
        else
        {
            var nextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, queuedEmail.RetryCount + 1));
            await repository.IncrementRetryCountAsync(queuedEmail.Id, nextRetryAt);
            
            _logger.LogWarning("Email {EmailId} failed (attempt {RetryCount}), will retry at {NextRetryAt}: {Error}", 
                queuedEmail.Id, queuedEmail.RetryCount + 1, nextRetryAt, errorMessage);
        }
    }

    private async Task PerformCleanup()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
            
            await repository.CleanupOldEmailsAsync(TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during email cleanup");
        }
    }

    private async Task ResetExpiredLocks()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
            
            await repository.ResetExpiredLocksAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured when resetting expired locks");
        }
    }

    public async Task<Guid> EnqueueAsync(EmailDetail email)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
        
        return await repository.EnqueueEmailAsync(email);
    }

    public override void Dispose()
    {
        _cleanupTimer.Dispose();
        _lockResetTimer.Dispose();
        base.Dispose();
    }
}