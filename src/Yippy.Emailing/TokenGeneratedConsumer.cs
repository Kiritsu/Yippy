using Yippy.Common.Identity;
using Yippy.Messaging;

namespace Yippy.Emailing;

public class TokenGeneratedConsumer(EmailingService emailingService) : IMessageConsumer<TokenGeneratedMessage>
{
    public Task HandleAsync(TokenGeneratedMessage message, CancellationToken cancellationToken = default)
    {
        // todo: call the gRPC template API to retrieve the token generated message template
        
        emailingService.Enqueue(new EmailDetail
        {
            From = new EmailDetail.EmailName("Yippy!", "noreply@alnmrc.com"),
            To = [new EmailDetail.EmailName(message.Email, message.Email)],
            Subject = "[Yippy!] Your access key has been generated",
            Body = $"Use this access-key to login to Yippy!: {message.AccessKey}",
            ContentType = "plain"
        });
        
        return Task.CompletedTask;
    }
}