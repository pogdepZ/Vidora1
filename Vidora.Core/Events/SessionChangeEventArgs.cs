using System;

namespace Vidora.Core.Events;

public enum SessionChangeReason
{
    ManualLogin,
    AutoRestore,
    TokenRefreshed,
    UserUpdated,
    ManualLogout,
    ForcedLogout,
    SessionExpired
}

public sealed class SessionChangeEventArgs : EventArgs
{
    public SessionChangeReason Reason { get; }

    public SessionChangeEventArgs(SessionChangeReason reason)
    {
        Reason = reason;
    }
}
