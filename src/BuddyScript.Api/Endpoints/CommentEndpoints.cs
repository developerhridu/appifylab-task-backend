using BuddyScript.Application.Comments.Create;
using BuddyScript.Application.Comments.ListForPost;
using MediatR;

namespace BuddyScript.Api.Endpoints;

public static class CommentEndpoints
{
    public record CreateCommentRequest(string Content, Guid? ParentCommentId);

    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts/{postId:guid}/comments").WithTags("Comments").RequireAuthorization();

        group.MapGet("/", async (Guid postId, ISender sender) =>
            Results.Ok(await sender.Send(new ListCommentsQuery(postId))));

        group.MapPost("/", async (Guid postId, CreateCommentRequest body, ISender sender) =>
        {
            var dto = await sender.Send(new CreateCommentCommand(postId, body.Content, body.ParentCommentId));
            return Results.Created($"/api/posts/{postId}/comments/{dto.Id}", dto);
        });

        return app;
    }
}
