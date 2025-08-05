using Microsoft.AspNetCore.Mvc;
using Yippy.Common.Identity;
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
            [FromBody] TokenValidationRequest request) =>
        {
            var user = await userService.ValidateAccessKeyAsync(request);
            if (user is null)
            {
                return Results.Problem("InvalidAccessKey", statusCode: 400);
            }

            var jwt = jwtService.GenerateToken(user);
            var refreshToken = await userService.CreateSecurityToken(user, 10080); // 7 days duration
            
            return Results.Ok(new AccessTokenResponse(jwt, refreshToken, 1800));
        });
    }
}

