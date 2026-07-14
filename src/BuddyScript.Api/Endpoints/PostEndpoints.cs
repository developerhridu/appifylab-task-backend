using BuddyScript.Application.Posts.Create;
using BuddyScript.Application.Posts.GetFeed;
using BuddyScript.Domain.Enums;
using MediatR;

namespace BuddyScript.Api.Endpoints;

public static class PostEndpoints
{
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Posts").RequireAuthorization();

        group.MapGet("/", async (string? cursor, int? limit, ISender sender) =>
        {
            var result = await sender.Send(new GetFeedQuery(cursor, limit ?? 10));
            return Results.Ok(result);
        });

        group.MapPost("/", async (HttpRequest request, ISender sender) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Expected multipart/form-data." });

            var form = await request.ReadFormAsync();
            var content = form["content"].ToString();

            var visibility = Enum.TryParse<Visibility>(form["visibility"], ignoreCase: true, out var v)
                ? v
                : Visibility.Public;

            var file = form.Files["image"];
            Stream? imageStream = null;
            string? imageName = null;

            if (file is not null)
            {
                if (file.Length > MaxImageBytes)
                    return Results.BadRequest(new { error = "Image exceeds 5 MB limit." });
                if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest(new { error = "Only image files are allowed." });

                imageStream = file.OpenReadStream();
                imageName = file.FileName;
            }

            var post = await sender.Send(new CreatePostCommand(content, visibility, imageStream, imageName));
            return Results.Created($"/api/posts/{post.Id}", post);
        }).DisableAntiforgery();

        return app;
    }
}
