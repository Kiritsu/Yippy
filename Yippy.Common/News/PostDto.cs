namespace Yippy.Common.News;

public class PostDto
{
    public required string Title { get; set; }

    public required string Body { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
}