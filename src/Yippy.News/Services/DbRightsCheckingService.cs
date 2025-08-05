using Microsoft.EntityFrameworkCore;
using Yippy.Common.Interfaces;
using Yippy.News.Data;

namespace Yippy.News.Services;

public class DbRightsCheckingService(YippyNewsDbContext db)
{
    public async Task<bool> HasRightsAsync<T>(Guid resourceId, Guid userId)
        where T : class, IResourceAuthor
    {
        return await db
            .Set<T>()
            .Where(x => x.UserId == userId && x.Id == resourceId)
            .AnyAsync();
    }
}