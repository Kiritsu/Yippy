using Microsoft.EntityFrameworkCore;
using Yippy.Common.Identity;
using Yippy.Identity.Data;

namespace Yippy.Identity.Services;

public interface IUserService
{
    Task<Guid?> CreateUserAccessAsync(EmailRequest request);
    
    Task<Guid> CreateSecurityToken(User user, int expiresInMinutes);
    
    Task<User?> ValidateAccessKeyAsync(TokenValidationRequest request);
}

public class UserService(YippyIdentityDbContext context, ILogger<UserService> logger) : IUserService
{
    public async Task<Guid?> CreateUserAccessAsync(EmailRequest request)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                user = new User { Email = request.Email, CreatedAtUtc = DateTime.UtcNow };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var accessKey = await CreateSecurityToken(user, 30);
            
            #if DEBUG
            // easier for debugging :D
            logger.TokenGenerated(accessKey);
            #endif
            
            await transaction.CommitAsync();

            return accessKey;
        }
        catch (Exception ex)
        {
            logger.ExceptionCreatingUserAccess(ex);
            await transaction.RollbackAsync();
            return null;
        }
    }

    public async Task<Guid> CreateSecurityToken(User user, int expiresInMinutes)
    {
        var accessKey = Guid.NewGuid();
        var userAccess = new UserAccess
        {
            UserId = user.Id,
            AccessKey = accessKey,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresInMinutes = expiresInMinutes
        };

        context.UserAccesses.Add(userAccess);
        await context.SaveChangesAsync();
        
        return accessKey;
    }

    public async Task<User?> ValidateAccessKeyAsync(TokenValidationRequest request)
    {
        var userAccess = await context.UserAccesses
            .Include(x => x.User)
            .Where(x => x.AccessKey == request.AccessKey)
            .FirstOrDefaultAsync();

        if (userAccess is null)
        {
            return null;
        }
        
        context.UserAccesses.Remove(userAccess);

        if (userAccess.ExpiresInMinutes > 0 && 
            DateTime.UtcNow - userAccess.CreatedAtUtc > TimeSpan.FromMinutes(userAccess.ExpiresInMinutes))
        {
            return null;
        }
        
        await context.SaveChangesAsync();
        
        return userAccess.User;
    }
}