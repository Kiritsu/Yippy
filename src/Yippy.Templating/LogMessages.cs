namespace Yippy.Templating;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Error resolving email template '{TemplateName}'")]
    public static partial void ErrorResolvingEmailTemplate(this ILogger logger, Exception ex, string templateName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Error resolving SMS template '{TemplateName}'")]
    public static partial void ErrorResolvingSmsTemplate(this ILogger logger, Exception ex, string templateName);
}
