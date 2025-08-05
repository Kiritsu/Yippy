namespace Yippy.Web.Authentication.Models;

public class AuthState
{
    public bool IsAuthenticated { get; set; }
    
    public string? Token { get; set; }
    
    public Guid? RefreshToken { get; set; }
    
    public DateTime TokenExpiry { get; set; }
    
    public string? Email { get; set; }
    
    public Dictionary<string, object> Claims { get; set; } = new();
}