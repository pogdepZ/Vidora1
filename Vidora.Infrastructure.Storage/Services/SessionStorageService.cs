using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Storage;
using Vidora.Core.ValueObjects;

namespace Vidora.Infrastructure.Storage.Services;

public class SessionStorageService : ISessionStorageService
{
    private const string SessionKey = "Session";
    private const string AccessTokenKey = "AccessToken";

    private readonly ILocalSettingsService _localSettings;
    private readonly ISecureVaultService _secureVault;
    public SessionStorageService(ILocalSettingsService localSettings, ISecureVaultService secureVault)
    {
        _localSettings = localSettings;
        _secureVault = secureVault;
    }

    public Session? LoadSession()
    {
        var stored = _localSettings.ReadSettings<Session>(SessionKey);
        if (stored == null)
            return null;

        var accessTokenValue = _secureVault.GetSecret(AccessTokenKey);
        if (string.IsNullOrWhiteSpace(accessTokenValue))
            return null;

        return new Session
        {
            CurrentUser = stored.CurrentUser,
            AccessToken = new AuthToken(accessTokenValue, stored.AccessToken.ExpiresAt),
        };
    }

    public void SaveSession(Session newSession)
    {
        var masked = new Session
        {
            CurrentUser = newSession.CurrentUser,
            AccessToken = new AuthToken("", newSession.AccessToken.ExpiresAt),
        };

        _localSettings.SaveSettings(SessionKey, masked);
        _secureVault.SaveSecret(AccessTokenKey, newSession.AccessToken.Token);
    }

    public void ClearSession()
    {
        _localSettings.RemoveSettings(SessionKey);
        _secureVault.RemoveSecret(AccessTokenKey);
    }
}
