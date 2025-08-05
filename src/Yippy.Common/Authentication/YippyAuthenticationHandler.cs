using System.Security.Claims;
using System.Text.Encodings.Web;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yippy.Identity;

namespace Yippy.Common.Authentication;

public class YippyAuthenticationOptions : AuthenticationSchemeOptions;

public class YippyAuthenticationHandler(
    IOptionsMonitor<YippyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TokenValidation.TokenValidationClient tokenValidationClient)
    : AuthenticationHandler<YippyAuthenticationOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "YippyAuth";
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return AuthenticateResult.Fail("Authentication required");
        }

        Metadata? metadata = null;
        if (Request.Headers.TryGetValue("X-Yippy-Trace", out var traceId))
        {
            metadata = new Metadata
            {
                { "X-Yippy-Trace", traceId.ToString() }
            };
        }
        
        var response = await tokenValidationClient.ValidateAsync(new JwtTokenValidationRequest { Token = authHeader }, metadata);
        if (!response.IsSuccess)
        {
            return AuthenticateResult.Fail("Authentication failed");
        }
        
        var identity = new ClaimsIdentity(response.Claims.Select(x => new Claim(x.Type, x.Value)), "Bearer");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
        
        return AuthenticateResult.Success(ticket);
    }
}