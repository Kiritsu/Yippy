using Grpc.Core;

namespace Yippy.Identity.Services;

public class TokenValidationService(JwtService jwtService) : TokenValidation.TokenValidationBase
{
    public override Task<JwtTokenValidationResponse> Validate(JwtTokenValidationRequest request, ServerCallContext context)
    {
        if (request.Token.StartsWith("Bearer "))
        {
            // removes "Bearer " from the token if present.
            request.Token = request.Token[7..];
        }
        
        if (!jwtService.ValidateToken(request.Token, out var jwt))
        {
            return Task.FromResult(new JwtTokenValidationResponse
            {
                IsSuccess = false
            });
        }

        var response = new JwtTokenValidationResponse
        {
            IsSuccess = true
        };
            
        response.Claims.AddRange(jwt.Claims.Select(x => new JwtTokenValidationResponse.Types.JwtClaim
        {
            Type = x.Type,
            Value = x.Value
        }));
            
        return Task.FromResult(response);
    }
}