using BuddyScript.Domain.Enums;
using MediatR;

namespace BuddyScript.Application.Posts.Create;

/// <summary>Image is passed as an already-opened stream (or null); the endpoint extracts it from the multipart form.</summary>
public record CreatePostCommand(
    string Content,
    Visibility Visibility,
    Stream? ImageStream,
    string? ImageFileName) : IRequest<PostDto>;
