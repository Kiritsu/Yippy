namespace Yippy.Emailing;

public class EmailDetail
{
    public required EmailName From { get; set; }

    public required EmailName[] To { get; set; }

    public required string Subject { get; set; }

    public required string Body { get; set; }

    public string ContentType { get; set; } = "html";

    public record EmailName(string Name, string Email);
}