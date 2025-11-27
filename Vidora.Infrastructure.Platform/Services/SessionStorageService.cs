using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Storage;
using Vidora.Core.ValueObjects;

namespace Vidora.Infrastructure.Platform.Services;

public class SessionStorageService : ISessionStorageService
{
    private const string SessionKey = "Session";
    private const string AccessKey = "AccessToken";
    private const string RefreshKey = "RefreshToken";

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

        var accessRaw = _secureVault.GetSecret(AccessKey);
        var refreshRaw = _secureVault.GetSecret(RefreshKey);
        if (string.IsNullOrEmpty(accessRaw) || string.IsNullOrEmpty(refreshRaw))
            return null;

        return new Session
        {
            CurrentUser = stored.CurrentUser,
            AccessToken = new AuthToken(accessRaw, stored.AccessToken.ExpiresAt),
            RefreshToken = new AuthToken(refreshRaw, stored.RefreshToken.ExpiresAt)
        };
    }

    public void SaveSession(Session newSession)
    {
        var masked = new Session
        {
            CurrentUser = newSession.CurrentUser,
            AccessToken = new AuthToken("", newSession.AccessToken.ExpiresAt),
            RefreshToken = new AuthToken("", newSession.RefreshToken.ExpiresAt)
        };

        _localSettings.SaveSettings(SessionKey, masked);
        _secureVault.SaveSecret(AccessKey, newSession.AccessToken.Token);
        _secureVault.SaveSecret(RefreshKey, newSession.RefreshToken.Token);
    }

    public void ClearSession()
    {
        _localSettings.RemoveSettings(SessionKey);
        _secureVault.RemoveSecret(AccessKey);
        _secureVault.RemoveSecret(RefreshKey);
    }
}
