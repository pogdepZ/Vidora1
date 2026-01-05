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

    [MemberNotNullWhen(true, nameof(CurrentSession), nameof(CurrentUser), nameof(AccessToken))]
    public bool IsAuthenticated => CurrentSession != null && !CurrentSession.AccessToken.IsExpired;


    private readonly ISessionStorageService _sessionStorage;
    public SessionStateService(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public void RestoreSession()
    {
        var restore = _sessionStorage.LoadSession();
        if (restore is null)
        {
            SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.ForcedLogout));
            return;
        }

        if (restore.AccessToken.IsExpired)
        {
            _sessionStorage.ClearSession();
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
        _sessionStorage.SaveSession(newSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(reason));
    }

    public void ClearSession(SessionChangeReason reason = SessionChangeReason.ManualLogout)
    {
        CurrentSession = null;
        _sessionStorage.ClearSession();
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(reason));
    }

    public void UpdateUser(User updatedUser)
    {
        if (!IsAuthenticated)
        {
            throw new InvalidOperationException("No valid session to update.");
        }

        CurrentSession.CurrentUser = updatedUser;
        _sessionStorage.SaveSession(CurrentSession);
        SessionChanged?.Invoke(this, new SessionChangeEventArgs(SessionChangeReason.UserUpdated));
    }
}
