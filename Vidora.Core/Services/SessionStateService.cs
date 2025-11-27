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
    public event EventHandler<SessionChangeEventArgs>? SessionChanged;

    public Session? CurrentSession { get; private set; }
    public User? CurrentUser => CurrentSession?.CurrentUser;
    public AuthToken? AccessToken => CurrentSession?.AccessToken;
    public AuthToken? RefreshToken => CurrentSession?.RefreshToken;

    [MemberNotNullWhen(true, nameof(CurrentSession), nameof(CurrentUser), nameof(RefreshToken))]
    public bool IsSessionValid => CurrentSession != null && !CurrentSession.RefreshToken.IsExpired;


    private readonly ISessionStorageService _sessionStorage;
    public SessionStateService(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public void RestoreSession()
    {
        var restore = _sessionStorage.LoadSession();
        if (restore != null)
        {
            CurrentSession = restore;
            SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.AutoRestore));
        }
    }

    public void SetSession(Session newSession, SessionChangeReason reason = SessionChangeReason.ManualLogin)
    {
        if (IsSessionValid)
        {
            throw new InvalidOperationException("A session is already active. Clear the current session before storing a new one.");
        }
        CurrentSession = newSession;
        _sessionStorage.SaveSession(newSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(reason));
    }

    public void UpdateUser(User updatedUser)
    {
        if (!IsSessionValid)
        {
            throw new InvalidOperationException("No valid session to update.");
        }

        CurrentSession.CurrentUser = updatedUser;
        _sessionStorage.SaveSession(CurrentSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.UserUpdated));
    }

    public void UpdateAccessToken(AuthToken newAccessToken)
    {
        if (!IsSessionValid)
        {
            throw new InvalidOperationException("No valid session to update.");
        }
        CurrentSession.AccessToken = newAccessToken;
        _sessionStorage.SaveSession(CurrentSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.TokenRefreshed));
    }


    public void ClearSession(SessionChangeReason reason = SessionChangeReason.ManualLogout)
    {
        CurrentSession = null;
        _sessionStorage.ClearSession();
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(reason));
    }
}
