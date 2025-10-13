using Microsoft.EntityFrameworkCore;
using Yippy.Common.News;
using Yippy.Common.Results;
using Yippy.News.Data;
using Yippy.News.Extensions;

namespace Yippy.News.Services;

public interface IPostService
{
    Task<Guid?> CreatePostAsync(PostCreateRequest request, Guid userId);
    Task<PostDto?> GetPostAsync(Guid id);
    Task<IYippyResult> DeletePostAsync(Guid id);
    Task<IYippyResult> UpdatePostAsync(Guid id, Guid revisionAuthorId, PostCreateRequest request);
}

public class PostService(YippyNewsDbContext context, ILogger<PostService> logger) : IPostService
{
    public async Task<PostDto?> GetPostAsync(Guid id)
    {
        try
        {
            var post = await context.GetPostByIdAsync(id);
            return post;
        }
        catch (Exception ex)
        {
            logger.ExceptionGettingPost(ex);
            return null;
        }
    }
    
    public async Task<Guid?> CreatePostAsync(PostCreateRequest request, Guid userId)
    {
        try
        {
            var post = new Post
            {
                Title = request.Title,
                Body = request.Body,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };

            context.Posts.Add(post);
            await context.SaveChangesAsync();

            return post.Id;
        }
        catch (Exception ex)
        {
            logger.ExceptionCreatingPost(ex);
            return null;
        }
    }
    
    public async Task<IYippyResult> DeletePostAsync(Guid id)
    {
        try
        {
            var post = await context.GetPostByIdAsync(id);
            if (post is null)
            {
                logger.DeletePostNotFound(id);
                return EmptyResult.Instance;
            }

            var count = await context.Posts
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();

            if (count <= 0)
            {
                logger.DeletePostFailed(id, count);
            }
            
            return EmptyResult.Instance;
        }
        catch (Exception ex)
        {
            logger.ExceptionDeletingPost(ex, id);
            return new FailedResult("Exception", $"An exception occured when deleting the post {id}");
        }
    }

    public async Task<IYippyResult> UpdatePostAsync(Guid id, Guid revisionAuthorId, PostCreateRequest request)
    {
        try
        {
            var post = await context.Posts.FirstOrDefaultAsync(x => x.Id == id);
            if (post is null)
            {
                return new FailedResult("NotFound", "The post was not found");
            }

            post.Title = request.Title;
            post.Body = request.Body;

            context.PostRevisions.Add(new PostRevision
            {
                PostId = post.Id,
                UserId = revisionAuthorId,
                CreatedAtUtc = DateTime.UtcNow
            });
            
            await context.SaveChangesAsync();
            return EmptyResult.Instance;
        }
        catch (Exception ex)
        {
            logger.ExceptionUpdatingPost(ex, id);
            return new FailedResult("Exception", $"An exception occured when updating the post {id}");
        }
    }
}