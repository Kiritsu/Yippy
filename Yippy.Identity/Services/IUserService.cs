using Yippy.Common.Identity;
using Yippy.Identity.Data;

namespace Yippy.Identity.Services;

public interface IUserService
{
    Task<Guid?> CreateUserAccessAsync(EmailRequest request);
    
    Task<Guid> CreateSecurityToken(User user, int expiresInMinutes);
    
    Task<User?> ValidateAccessKeyAsync(TokenValidationRequest request);
}