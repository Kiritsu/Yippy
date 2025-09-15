using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yippy.Identity;

namespace Yippy.Common.Authentication;

public static class YippyAuthenticationExtensions
{
    public static void AddYippyAuthentication(this WebApplicationBuilder @this, Action<AuthorizationOptions>? authorizeAction = null)
    {
        @this.Services.AddGrpcClient<TokenValidation.TokenValidationClient>(
            opt => opt.Address = new Uri(@this.Configuration["Backends:Identity"]!));
        
        @this.Services.AddAuthentication(YippyAuthenticationHandler.AuthenticationScheme)
            .AddScheme<YippyAuthenticationOptions, YippyAuthenticationHandler>(
                YippyAuthenticationHandler.AuthenticationScheme, null);

        if (authorizeAction != null)
        {
            @this.Services.AddAuthorization(authorizeAction);
        }
        else
        {
            @this.Services.AddAuthorization();
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