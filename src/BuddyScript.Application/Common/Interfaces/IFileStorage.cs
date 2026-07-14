namespace BuddyScript.Application.Common.Interfaces;

/// <summary>Abstracts image persistence (implemented by Infrastructure — Cloudinary).</summary>
public interface IFileStorage
{
    /// <summary>Uploads an image and returns its public URL.</summary>
    Task<string> UploadImageAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
}
