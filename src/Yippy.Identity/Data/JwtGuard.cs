namespace Yippy.Identity.Data;

public class JwtGuard
{
    public Guid Id { get; set; }
    
    public required string TokenHashSha256 { get; set; }
    
    public required DateTime RevokedAtUtc { get; set; }
    
    public required DateTime TokenExpiresAtUtc { get; set; }
}