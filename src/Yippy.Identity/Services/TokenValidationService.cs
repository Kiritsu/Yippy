using Grpc.Core;

namespace Yippy.Identity.Services;

public class TokenValidationService(JwtService jwtService, JwtRevocationService revocationService) : TokenValidation.TokenValidationBase
{
    public override async Task<JwtTokenValidationResponse> Validate(JwtTokenValidationRequest request, ServerCallContext context)
    {
        if (request.Token.StartsWith("Bearer "))
        {
            // removes "Bearer " from the token if present.
            request.Token = request.Token[7..];
        }
        
        if (!jwtService.ValidateToken(request.Token, out var jwt))
        {
            return new JwtTokenValidationResponse
            {
                IsSuccess = false
            };
        }

        if (await revocationService.IsTokenRevokedAsync(request.Token, context.CancellationToken))
        {
            return new JwtTokenValidationResponse
            {
                IsSuccess = false
            };
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
            
        return response;
    }
}