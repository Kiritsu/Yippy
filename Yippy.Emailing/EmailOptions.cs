namespace Yippy.Emailing;

public class EmailOptions
{
    public required string Hostname { get; set; }

    public required int Port { get; set; }

    public required bool UseSsl { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }
}