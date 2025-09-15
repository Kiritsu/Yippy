namespace Yippy.Templating.Data;

public class SmsTemplate
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string FromName { get; set; }
    
    public required string Body { get; set; }
}