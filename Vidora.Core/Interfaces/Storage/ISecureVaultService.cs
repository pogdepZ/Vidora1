namespace Vidora.Core.Interfaces.Storage;

public interface ISecureVaultService
{
    string? GetSecret(string key);
    void RemoveSecret(string key);
    void SaveSecret(string key, string value);
}
