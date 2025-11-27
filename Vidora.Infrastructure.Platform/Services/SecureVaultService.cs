using Vidora.Core.Interfaces.Storage;
using Windows.Security.Credentials;

namespace Vidora.Infrastructure.Platform.Services;

public class SecureVaultService : ISecureVaultService
{
    private const string ResourceName = "Vidora.SecureResource";

    public void SaveSecret(string key, string value)
    {
        var passwordVault = new PasswordVault();

        try
        {
            var existingCredential = passwordVault.Retrieve(ResourceName, key);
            passwordVault.Remove(existingCredential);
        }
        catch
        {
            // Ignore
        }

        var newCredential = new PasswordCredential(ResourceName, key, value);
        passwordVault.Add(newCredential);
    }

    public string? GetSecret(string key)
    {
        try
        {
            var passwordVault = new PasswordVault();
            var retrievedCredential = passwordVault.Retrieve(ResourceName, key);
            retrievedCredential.RetrievePassword();
            return retrievedCredential.Password;
        }
        catch
        {
            return null;
        }
    }

    public void RemoveSecret(string key)
    {
        try
        {
            var passwordVault = new PasswordVault();
            var retrievedCredential = passwordVault.Retrieve(ResourceName, key);
            passwordVault.Remove(retrievedCredential);
        }
        catch
        {
            // Ignore
        }
    }
}
