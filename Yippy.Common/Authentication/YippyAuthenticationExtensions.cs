using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Yippy.Common.Authentication;

public static class YippyAuthenticationExtensions
{
    public static void AddYippyAuthentication(this IServiceCollection @this, Action<AuthorizationOptions>? authorizeAction = null)
    {
        @this.AddAuthentication("YippyAuth")
            .AddScheme<YippyAuthenticationOptions, YippyAuthenticationHandler>("YippyAuth", null);

        if (authorizeAction != null)
        {
            @this.AddAuthorization(authorizeAction);
        }
        else
        {
            @this.AddAuthorization();
        }
    }

    public static void UseYippyAuthentication(this WebApplication @this)
    {
        @this.UseAuthentication();
        @this.UseAuthorization();
    }
    
    public static Guid? GetAuthenticatedUserId(this ClaimsPrincipal @this) =>
        Guid.TryParse(@this.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
}