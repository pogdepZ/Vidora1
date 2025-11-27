using System.Threading.Tasks;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Contracts.Services;

public interface IUserCredentialsService
{
    Task<bool> VerifyIdentityAsync(string? message = null);
    Task<Credentials?> GetCredentialsAsync(bool requireVerification = true, string? message = null);
    Task SaveCredentialsAsync(string email, string password);
    Task ClearCredentialsAsync();
}
