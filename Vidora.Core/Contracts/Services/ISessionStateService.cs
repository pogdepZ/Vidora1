using System;
using System.Diagnostics.CodeAnalysis;
using Vidora.Core.Entities;
using Vidora.Core.Events;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Contracts.Services;

public interface ISessionStateService
{
    event EventHandler<SessionChangeEventArgs>? SessionChanged;

    Session? CurrentSession { get; }
    User? CurrentUser { get; }
    AuthToken? AccessToken { get; }
    AuthToken? RefreshToken { get; }

    [MemberNotNullWhen(true, nameof(CurrentSession), nameof(CurrentUser), nameof(RefreshToken))]
    bool IsSessionValid { get; }

    void RestoreSession();
    void SetSession(Session newSession, SessionChangeReason reason = SessionChangeReason.ManualLogin);
    void UpdateUser(User updatedUser);
    void UpdateAccessToken(AuthToken newAccessToken);
    void ClearSession(SessionChangeReason reason = SessionChangeReason.ManualLogout);
}