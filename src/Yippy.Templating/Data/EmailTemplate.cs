namespace Yippy.Templating.Data;

public class EmailTemplate
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string FromName { get; set; }

    public required string FromEmail { get; set; }
    
    public required string Subject { get; set; }
    
    public required string Body { get; set; }
}