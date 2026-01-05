using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Vidora.Core.Interfaces.Services;
using Vidora.Infrastructure.Api.Configuration;

namespace Vidora.Infrastructure.Api.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName, string? folder = null)
    {
        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder ?? "vidora/avatars",
                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return Result.Failure<string>($"Upload failed: {uploadResult.Error.Message}");
            }

            return Result.Success(uploadResult.SecureUrl.ToString());
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Upload failed: {ex.Message}");
        }
    }

    public async Task<Result> DeleteImageAsync(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Error != null)
            {
                return Result.Failure($"Delete failed: {result.Error.Message}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Delete failed: {ex.Message}");
        }
    }
}
