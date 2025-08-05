namespace Yippy.Identity.Data;

public class UserAccess
{
    public Guid Id { get; set; }
    
    public required Guid UserId { get; set; }
    public User? User { get; set; }

    public required Guid AccessKey { get; set; }
    
    public required DateTime CreatedAtUtc { get; set; }

    public int ExpiresInMinutes { get; set; }
}