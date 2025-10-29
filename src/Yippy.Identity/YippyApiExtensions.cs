using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yippy.Common.Identity;
using Yippy.Identity.Configuration;
using Yippy.Identity.Services;
using Yippy.Messaging;

namespace Yippy.Identity;

public static class YippyApiExtensions
{
    public static void MapYippyApi(this WebApplication @this)
    {
        @this.MapPost("/auth/token", async (
            [FromServices] IUserService userService, 
            [FromServices] IMessagePublisher<TokenGeneratedMessage> tokenGeneratedPublisher, 
            [FromBody] EmailRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.Problem("InvalidEmail", statusCode: 400);
            }

            var token = await userService.CreateUserAccessAsync(request);
            if (token == null)
            {
                return Results.Problem("TokenFailure");
            }

            await tokenGeneratedPublisher.PublishAsync(new TokenGeneratedMessage
            {
                Email = request.Email,
                AccessKey = token.Value
            });
            
            return Results.Accepted();
        });

        @this.MapPost("/auth/validate", async (
            [FromServices] IUserService userService, 
            [FromServices] JwtService jwtService,
            [FromServices] IOptions<JwtOptions> jwtOptions,
            [FromBody] TokenValidationRequest request) =>
        {
            var user = await userService.ValidateAccessKeyAsync(request);
            if (user is null)
            {
                return Results.Problem("InvalidAccessKey", statusCode: 400);
            }

            var jwt = jwtService.GenerateToken(user);
            var refreshToken = await userService.CreateSecurityToken(user, 10080); // 7 days duration
            
            return Results.Ok(new AccessTokenResponse(jwt, refreshToken, jwtOptions.Value.ExpirationMinutes * 60));
        });

        @this.MapPost("/auth/logout", async (
            [FromServices] JwtService jwtService,
            [FromServices] JwtRevocationService revocationService,
            [FromHeader(Name = "Authorization")] string? authorization) =>
        {
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return Results.Problem("NoAuthorization", statusCode: 400);
            }

            var token = authorization.StartsWith("Bearer ")
                ? authorization[7..]
                : authorization;

            if (!jwtService.ValidateToken(token, out var jwt))
            {
                return Results.Problem("InvalidToken", statusCode: 400);
            }

            await revocationService.RevokeTokenAsync(token, jwt.ValidTo);

            return Results.NoContent();
        });
    }
}

