namespace Yippy.Web.Authentication.Models;

public record AccessTokenResponse(string Token, Guid RefreshToken, int Duration);