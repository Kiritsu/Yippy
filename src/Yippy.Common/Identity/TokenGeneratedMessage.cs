using Yippy.Messaging;

namespace Yippy.Common.Identity;

public class TokenGeneratedMessage : IMessage
{
    public required string Email { get; set; }

    public required Guid AccessKey { get; set; }
}