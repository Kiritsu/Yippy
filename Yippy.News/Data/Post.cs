using Yippy.Common.Interfaces;

namespace Yippy.News.Data;

public class Post : IResourceAuthor
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    
    public required string Title { get; set; }

    public required string Body { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public ICollection<PostRevision>? Revisions { get; set; }
}

public class PostRevision
{
    public Guid Id { get; set; }
    
    public Guid PostId { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }

    public Guid UserId { get; set; }
}