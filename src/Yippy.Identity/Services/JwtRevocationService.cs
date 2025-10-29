using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Yippy.Identity.Data;

namespace Yippy.Identity.Services;

public class JwtRevocationService(
    YippyIdentityDbContext dbContext,
    HybridCache cache)
{
    private const string CacheKeyPrefix = "jwt:revoked:";
    
    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        var tokenHash = ComputeTokenHash(token);
        var cacheKey = $"{CacheKeyPrefix}{tokenHash}";
        
        var isRevoked = await cache.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                return await dbContext.JwtGuards
                    .AsNoTracking()
                    .AnyAsync(x => x.TokenHashSha256 == tokenHash, cancellationToken);
            },
            cancellationToken: cancellationToken);
            
        return isRevoked;
    }
    
    public async Task RevokeTokenAsync(string token, DateTime tokenExpiresAtUtc, CancellationToken cancellationToken = default)
    {
        if (await IsTokenRevokedAsync(token, cancellationToken))
        {
            return;
        }
        
        var tokenHash = ComputeTokenHash(token);
        var cacheKey = $"{CacheKeyPrefix}{tokenHash}";
        var now = DateTime.UtcNow;
        
        // todo: make cleanup done smarter (in a background job?)
        await dbContext.JwtGuards
            .Where(x => x.TokenExpiresAtUtc < now)
            .ExecuteDeleteAsync(cancellationToken);
        
        var jwtGuard = new JwtGuard
        {
            Id = Guid.NewGuid(),
            TokenHashSha256 = tokenHash,
            RevokedAtUtc = now,
            TokenExpiresAtUtc = tokenExpiresAtUtc
        };
        
        dbContext.JwtGuards.Add(jwtGuard);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await cache.SetAsync(cacheKey, true, cancellationToken: cancellationToken);
    }
    
    private static string ComputeTokenHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}