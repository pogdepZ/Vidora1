using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Interfaces.Storage;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Services;

public class UserCredentialsService : IUserCredentialsService
{
    private const string EmailKey = "Email";
    private const string PasswordKey = "Password";
    private const string DefaultConsentMessage = "Verify your identity to access saved credentials";

    private readonly ISecureVaultService _secureVault;
    private readonly IIdentityVerificationService _identityVerification;
    public UserCredentialsService(ISecureVaultService secureVault, IIdentityVerificationService identityVerification)
    {
        _secureVault = secureVault;
        _identityVerification = identityVerification;
    }

    //
    public async Task<bool> VerifyIdentityAsync(string? message = null)
    {
        var available = await _identityVerification.IsAvailableAsync();
        if (!available)
            return false;

        var verified = await _identityVerification.RequestConsentAsync(message ?? DefaultConsentMessage);
        return verified;
    }

    public async Task<Credentials?> GetCredentialsAsync(bool requireVerification = false, string? message = null)
    {
        var email = _secureVault.GetSecret(EmailKey);
        var password = _secureVault.GetSecret(PasswordKey);

        if (email is null || password is null)
            return null;

        if (!Credentials.TryCreate(email, password, out var creds, out var error))
        {
            return null;
        }

        if (requireVerification)
        {
            var verified = await VerifyIdentityAsync(message);
            if (!verified)
                return null;
        }

        return creds;
    }

    public Task SaveCredentialsAsync(string email, string password)
    {
        if (Credentials.TryCreate(email, password, out var _, out var _))
        {
            _secureVault.SaveSecret(EmailKey, email);
            _secureVault.SaveSecret(PasswordKey, password);
        }

        return Task.CompletedTask;
    }

    public Task ClearCredentialsAsync()
    {
        _secureVault.RemoveSecret(EmailKey);
        _secureVault.RemoveSecret(PasswordKey);
        return Task.CompletedTask;
    }
}
