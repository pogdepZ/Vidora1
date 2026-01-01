using System;

namespace Vidora.Core.Events;

public sealed class SessionChangeEventArgs : EventArgs
{
    public SessionChangeReason Reason { get; }

    public SessionChangeEventArgs(SessionChangeReason reason)
    {
        Reason = reason;
    }
}
