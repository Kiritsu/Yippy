namespace Yippy.Common.Identity;

public record AccessTokenResponse(string Token, Guid RefreshToken, int Duration);