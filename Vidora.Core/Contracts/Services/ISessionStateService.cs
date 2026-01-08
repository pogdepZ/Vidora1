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

    [MemberNotNullWhen(true, nameof(CurrentUser), nameof(AccessToken))]
    bool IsAuthenticated { get; }

    void UpdateUser(User updatedUser);
    void RestoreSession();
    void SetSession(Session newSession, SessionChangeReason reason = SessionChangeReason.ManualLogin);
    void ClearSession(SessionChangeReason reason = SessionChangeReason.ManualLogout);
}