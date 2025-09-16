using Yippy.Common.Identity;
using Yippy.Messaging;
using Yippy.Templating;

namespace Yippy.Emailing;

public class TokenGeneratedConsumer(
    EmailingService emailingService, 
    TemplateResolver.TemplateResolverClient templateResolver, 
    ILogger<TokenGeneratedConsumer> logger) 
    : IMessageConsumer<TokenGeneratedMessage>
{
    public async Task HandleAsync(TokenGeneratedMessage message, CancellationToken cancellationToken = default)
    {
        var emailTemplate = await templateResolver.GetEmailAsync(new ResolveEmailTemplateRequest
        {
            TemplateName = "TOKEN_CREATED",
            Variables = { ["ACCESS_KEY"] = $"{message.AccessKey}" }
        }, cancellationToken: cancellationToken);

        if (emailTemplate is null)
        {
            throw new InvalidOperationException("Failed to retrieve the email template for TOKEN_CREATED");
        }
        
        var id = await emailingService.EnqueueAsync(new EmailDetail
        {
            From = new EmailDetail.EmailName(emailTemplate.FromName, emailTemplate.FromEmail),
            To = [new EmailDetail.EmailName(message.Email, message.Email)],
            Subject = emailTemplate.Object,
            Body = emailTemplate.RawBody,
            ContentType = "html"
        });
        
        logger.LogInformation("Enqueued email {Id} into the database", id);
    }
}