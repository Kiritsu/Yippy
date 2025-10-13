using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Yippy.Web.Authentication.Models;

namespace Yippy.Web.Authentication;

public class YippyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;
    private readonly ILogger<YippyAuthenticationStateProvider> _logger;

    public YippyAuthenticationStateProvider(
        IAuthService authService,
        ILogger<YippyAuthenticationStateProvider> logger)
    {
        _authService = authService;
        _logger = logger;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var authState = await _authService.GetAuthStateAsync();

            if (authState is {IsAuthenticated: true} && !string.IsNullOrEmpty(authState.Token))
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, authState.Email ?? ""),
                    new(ClaimTypes.Email, authState.Email ?? ""),
                    new("token", authState.Token)
                };
                
                claims.AddRange(authState.Claims
                    .Select(claim => new Claim(claim.Key, claim.Value.ToString() ?? "")));

                var identity = new ClaimsIdentity(claims, "custom");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
        }
        catch (Exception ex)
        {
            _logger.ErrorGettingAuthState(ex);
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private void OnAuthStateChanged(AuthState? authState)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
}