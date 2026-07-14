using BuddyScript.Application.Common.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BuddyScript.Infrastructure.Storage;

public class CloudinaryStorage : IFileStorage
{
    private readonly Cloudinary _cloudinary;
    private readonly string _folder;

    public CloudinaryStorage(IOptions<CloudinarySettings> options)
    {
        var s = options.Value;
        _cloudinary = new Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret)) { Api = { Secure = true } };
        _folder = s.Folder;
    }

    public async Task<string> UploadImageAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, content),
            Folder = _folder,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
            throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}
