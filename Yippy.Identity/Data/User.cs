namespace Yippy.Identity.Data;

public class User
{
    public Guid Id { get; set; }
    
    public required string Email { get; set; }

    public required DateTime CreatedAtUtc { get; set; }

    public ICollection<UserAccess>? UserAccesses { get; set; }
}
