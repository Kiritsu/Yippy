namespace Yippy.Web.Models;

public class PostDto
{
    public required string Title { get; set; }

    public required string Body { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public List<PostRevisionDto> Revisions { get; set; } = [];
}

public class PostRevisionDto
{
    public DateTime CreatedAtUtc { get; set; }

    public Guid UserId { get; set; }
}