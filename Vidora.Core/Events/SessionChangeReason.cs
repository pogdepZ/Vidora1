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