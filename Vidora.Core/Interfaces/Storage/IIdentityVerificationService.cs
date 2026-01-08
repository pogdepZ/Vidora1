using System.Threading.Tasks;

namespace Vidora.Core.Interfaces.Storage;

public interface IIdentityVerificationService
{
    Task<bool> IsAvailableAsync();
    Task<bool> RequestConsentAsync(string message);
}