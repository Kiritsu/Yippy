using System.Threading.Channels;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Yippy.Emailing;

public class EmailingService(IOptions<EmailOptions> options, ILogger<EmailingService> logger) : BackgroundService
{
    private readonly Channel<EmailDetail> _emailQueue = Channel.CreateUnbounded<EmailDetail>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // todo: implement something resilient
        // proposal :
        //  -> dequeue and insert into a psql database
        //  -> update status when done or failed
        //  -> poll (at service startup and sometimes during service lifetime) for unprocessed lines
        //     and immediatly lock to avoid concurrency issues if the service has multiple instances 
        //  -> cleanup the table because we don't want to keep traces
        
        using var smtpClient = new SmtpClient();
        
        await foreach (var email in _emailQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await smtpClient.ConnectAsync(options.Value.Hostname, options.Value.Port, options.Value.UseSsl,
                    stoppingToken);
                await smtpClient.AuthenticateAsync(options.Value.Username, options.Value.Password, stoppingToken);

                var message = new MimeMessage
                {
                    From = { email.From.ToMailboxAddress() },
                    Subject = email.Subject,
                    Body = new TextPart(email.ContentType)
                    {
                        Text = email.Body
                    }
                };

                message.To.AddRange(email.To.Select(x => x.ToMailboxAddress()));

                await smtpClient.SendAsync(message, stoppingToken);
                await smtpClient.DisconnectAsync(true, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occured when attempting to send an email");
                return;
            }
        }
    }

    public void Enqueue(EmailDetail email)
    {
        _emailQueue.Writer.TryWrite(email);
    }
}

public class EmailDetail
{
    public required EmailName From { get; set; }

    public required EmailName[] To { get; set; }

    public required string Subject { get; set; }

    public required string Body { get; set; }

    public string ContentType { get; set; } = "html";

    public record EmailName(string Name, string Email);
}