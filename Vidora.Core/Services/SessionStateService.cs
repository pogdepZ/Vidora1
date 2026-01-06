using System;
using System.Diagnostics.CodeAnalysis;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Events;
using Vidora.Core.Interfaces.Storage;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Services;

public class SessionStateService : ISessionStateService
{
    private const string SessionKey = "Session";
    private const string AccessTokenKey = "AccessToken";

    public event EventHandler<SessionChangeEventArgs>? SessionChanged;

    public Session? CurrentSession { get; private set; }
    public User? CurrentUser => CurrentSession?.CurrentUser;
    public AuthToken? AccessToken => CurrentSession?.AccessToken;

    [MemberNotNullWhen(true, nameof(CurrentSession), nameof(CurrentUser), nameof(AccessToken))]
    public bool IsAuthenticated => CurrentSession != null && !CurrentSession.AccessToken.IsExpired;

    private readonly ILocalSettingsService _localSettings;
    private readonly ISecureVaultService _secureVault;

    public SessionStateService(ILocalSettingsService localSettings, ISecureVaultService secureVault)
    {
        _localSettings = localSettings;
        _secureVault = secureVault;
    }

    public void RestoreSession()
    {
        var restore = LoadSession();
        if (restore is null)
        {
            SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.ForcedLogout));
            return;
        }

        if (restore.AccessToken.IsExpired)
        {
            _localSettings.RemoveSettings(SessionKey);
            _secureVault.RemoveSecret(AccessTokenKey);

            SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.SessionExpired));
            return;
        }

        CurrentSession = restore;
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.AutoRestore));
    }

    public void SetSession(Session newSession, SessionChangeReason reason = SessionChangeReason.ManualLogin)
    {
        if (IsAuthenticated)
        {
            throw new InvalidOperationException("A session is already active. Clear the current session before storing a new one.");
        }
        CurrentSession = newSession;
        SaveSession(newSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(reason));
    }

    public void ClearSession(SessionChangeReason reason = SessionChangeReason.ManualLogout)
    {
        CurrentSession = null;
        
        _localSettings.RemoveSettings(SessionKey);
        _secureVault.RemoveSecret(AccessTokenKey);

        SessionChanged?.Invoke(this, new SessionChangeEventArgs(reason));
    }

    public void UpdateUser(User updatedUser)
    {
        if (!IsAuthenticated)
        {
            throw new InvalidOperationException("No valid session to update.");
        }

        CurrentSession.CurrentUser = updatedUser;
        SaveSession(CurrentSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.UserUpdated));
    }

    private Session? LoadSession()
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

    private void SaveSession(Session newSession)
    {
        var masked = new Session
        {
            CurrentUser = newSession.CurrentUser,
            AccessToken = new AuthToken("", newSession.AccessToken.ExpiresAt),
        };

        _localSettings.SaveSettings(SessionKey, masked);
        _secureVault.SaveSecret(AccessTokenKey, newSession.AccessToken.Token);
    }
}
