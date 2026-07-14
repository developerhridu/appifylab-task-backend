using BuddyScript.Application.Likes.SetCommentLike;
using BuddyScript.Application.Likes.SetPostLike;
using BuddyScript.Application.Likes.WhoLikedComment;
using BuddyScript.Application.Likes.WhoLikedPost;
using MediatR;

namespace BuddyScript.Api.Endpoints;

public static class LikeEndpoints
{
    public static IEndpointRouteBuilder MapLikeEndpoints(this IEndpointRouteBuilder app)
    {
        var posts = app.MapGroup("/api/posts").WithTags("Likes").RequireAuthorization();

        posts.MapPost("/{id:guid}/like", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new SetPostLikeCommand(id, Like: true))));

        posts.MapDelete("/{id:guid}/like", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new SetPostLikeCommand(id, Like: false))));

        posts.MapGet("/{id:guid}/likes", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new WhoLikedPostQuery(id))));

        var comments = app.MapGroup("/api/comments").WithTags("Likes").RequireAuthorization();

        comments.MapPost("/{id:guid}/like", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new SetCommentLikeCommand(id, Like: true))));

        comments.MapDelete("/{id:guid}/like", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new SetCommentLikeCommand(id, Like: false))));

        comments.MapGet("/{id:guid}/likes", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new WhoLikedCommentQuery(id))));

        return app;
    }
}
