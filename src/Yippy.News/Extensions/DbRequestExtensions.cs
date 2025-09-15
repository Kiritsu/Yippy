using Microsoft.EntityFrameworkCore;
using Yippy.Common.News;
using Yippy.News.Data;

namespace Yippy.News.Extensions;

public static class DbRequestExtensions
{
    private static readonly Func<YippyNewsDbContext, Guid, Task<PostDto?>> GetPostById
        = EF.CompileAsyncQuery(
            (YippyNewsDbContext ctx, Guid id) => ctx.Posts
                .Where(x => x.Id == id)
                .Include(x => x
                    .Revisions!
                    .OrderByDescending(y => y.CreatedAtUtc)
                    .Take(5))
                .Select(x => new PostDto
                {
                    Title = x.Title,
                    Body = x.Body,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Revisions = x
                        .Revisions!
                        .Select(y => new PostRevisionDto
                        {
                            CreatedAtUtc = y.CreatedAtUtc, 
                            UserId = y.UserId
                        })
                        .ToList()
                })
                .FirstOrDefault());
    
    public static async Task<PostDto?> GetPostByIdAsync(this YippyNewsDbContext context, Guid id)
        => await GetPostById(context, id);
}