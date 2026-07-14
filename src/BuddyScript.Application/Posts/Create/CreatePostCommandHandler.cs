using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using BuddyScript.Domain.Entities;
using MediatR;

namespace BuddyScript.Application.Posts.Create;

public class CreatePostCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IFileStorage fileStorage) : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        string? imageUrl = null;
        if (request.ImageStream is not null)
            imageUrl = await fileStorage.UploadImageAsync(request.ImageStream, request.ImageFileName ?? "upload", cancellationToken);

        var post = new Post
        {
            AuthorId = userId,
            Content = request.Content?.Trim() ?? string.Empty,
            ImagePath = imageUrl,
            Visibility = request.Visibility
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync(cancellationToken);

        // Author is the current user; fetch name for the DTO without a round-trip through navigation.
        var author = await db.Users.FindAsync([userId], cancellationToken);

        return new PostDto(
            post.Id,
            new UserMini(userId, author!.FirstName, author.LastName),
            post.Content,
            post.ImagePath,
            post.Visibility,
            post.CreatedAt,
            LikeCount: 0,
            LikedByMe: false,
            CommentCount: 0);
    }
}
