using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Yippy.Common.Authentication;
using Yippy.Common.News;
using Yippy.News.Data;
using Yippy.News.Services;

namespace Yippy.News;

public static class YippyApiExtensions
{
    public static void MapYippyApi(this WebApplication @this)
    {
        var postsGroup = @this.MapGroup("/posts").RequireAuthorization();
        
        postsGroup.MapPost("", async (
            ClaimsPrincipal user,
            [FromServices] IPostService postService,
            [FromBody] PostCreateRequest request) =>
        {
            var userId = user.GetAuthenticatedUserId();

            var postId = await postService.CreatePostAsync(request, userId!.Value);
            return postId.HasValue 
                ? Results.Ok(new { Id = postId.Value }) 
                : Results.Problem("PostCreationFailure");
        }).RequireAuthorization();

        postsGroup.MapGet("/{id:guid}", async ([FromServices] IPostService postService, Guid id) =>
        {
            var post = await postService.GetPostAsync(id);
            if (post is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(post);
        });

        postsGroup.MapDelete("/{id:guid}", async (
            ClaimsPrincipal user,
            [FromServices] IPostService postService, 
            [FromServices] DbRightsCheckingService dbRightsCheckingService, 
            Guid id) =>
        {
            var hasRights = await dbRightsCheckingService
                .HasRightsAsync<Post>(id, user.GetAuthenticatedUserId().GetValueOrDefault());

            if (!hasRights)
            {
                return Results.Forbid();
            }
            
            var result = await postService.DeletePostAsync(id);
            return result.Success ? Results.NoContent() : Results.Problem(result.Key);
        });

        postsGroup.MapPatch("/{id:guid}", async (
            ClaimsPrincipal user,
            [FromServices] IPostService postService,
            [FromServices] DbRightsCheckingService dbRightsCheckingService,
            [FromBody] PostCreateRequest request,
            Guid id) =>
        {
            var userId = user.GetAuthenticatedUserId().GetValueOrDefault();
            
            var hasRights = await dbRightsCheckingService
                .HasRightsAsync<Post>(id, userId);

            if (!hasRights)
            {
                return Results.Forbid();
            }
            
            var result = await postService.UpdatePostAsync(id, userId, request);
            return result.Success ? Results.NoContent() : Results.Problem(result.Key);
        });
    }
}