using Vidora.Core.Entities;

namespace Vidora.Core.Interfaces.Storage;

public interface ISessionStorageService
{
    Session? LoadSession();
    void SaveSession(Session newSession);
    void ClearSession();
}
