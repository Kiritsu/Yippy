using Yippy.Common.Identity;
using Yippy.Messaging;

namespace Yippy.Emailing;

public class TokenGeneratedConsumer(EmailingService emailingService, ILogger<TokenGeneratedConsumer> logger) 
    : IMessageConsumer<TokenGeneratedMessage>
{
    public async Task HandleAsync(TokenGeneratedMessage message, CancellationToken cancellationToken = default)
    {
        // todo: call the gRPC template API to retrieve the token generated message template
        
        var id = await emailingService.EnqueueAsync(new EmailDetail
        {
            From = new EmailDetail.EmailName("Yippy!", "noreply@alnmrc.com"),
            To = [new EmailDetail.EmailName(message.Email, message.Email)],
            Subject = "[Yippy!] Your access key has been generated",
            Body = $"Use this access-key to login to Yippy!: {message.AccessKey}",
            ContentType = "plain"
        });
        
        logger.LogInformation("Enqueued email {Id} into the database", id);
    }
}