using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Yippy.Identity.Configuration;
using Yippy.Identity.Data;

namespace Yippy.Identity.Services;

public class JwtService
{
    private readonly JwtOptions _jwtOptions;
    
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly TokenValidationParameters _validationParameters;
    
    private readonly TimeSpan _expiration;
    
    public JwtService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        _expiration = TimeSpan.FromMinutes(_jwtOptions.ExpirationMinutes);
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
    
    public string GenerateToken(User user)
    {
        var claims = new Claim[2];
        claims[0] = new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
        claims[1] = new Claim(ClaimTypes.Email, user.Email);

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.Add(_expiration),
            signingCredentials: _signingCredentials);
        
        return _tokenHandler.WriteToken(token);
    }
    
    public bool ValidateToken(string token,
        [NotNullWhen(true)] out JwtSecurityToken? jwt)
    {
        try
        {
            _tokenHandler.ValidateToken(token, _validationParameters, out SecurityToken validatedToken);
            jwt = (JwtSecurityToken)validatedToken;
            return true;
        }
        catch (Exception)
        {
            jwt = null;
            return false;
        }
    }
}