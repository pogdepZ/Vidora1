using CSharpFunctionalExtensions;
using System.IO;
using System.Threading.Tasks;

namespace Vidora.Core.Interfaces.Services;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload ?nh lên Cloudinary
    /// </summary>
    /// <param name="fileStream">Stream c?a file ?nh</param>
    /// <param name="fileName">Tên file</param>
    /// <param name="folder">Thý m?c trên Cloudinary (optional)</param>
    /// <returns>URL c?a ?nh ð? upload</returns>
    Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName, string? folder = null);

    /// <summary>
    /// Xóa ?nh trên Cloudinary
    /// </summary>
    /// <param name="publicId">Public ID c?a ?nh</param>
    Task<Result> DeleteImageAsync(string publicId);
}
