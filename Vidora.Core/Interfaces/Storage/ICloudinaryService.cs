using CSharpFunctionalExtensions;
using System.IO;
using System.Threading.Tasks;

namespace Vidora.Core.Interfaces.Storage;

public interface ICloudinaryService
{
    Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName, string? folder = null);

    Task<Result> DeleteImageAsync(string publicId);
}
