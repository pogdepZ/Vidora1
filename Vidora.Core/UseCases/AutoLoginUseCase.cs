using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;

namespace Vidora.Core.UseCases;

public class AutoLoginUseCase
{
    private readonly ISessionStateService _sessionState;

    public AutoLoginUseCase(ISessionStateService sessionState)
    {
        _sessionState = sessionState; 
    }

    public Task ExecuteAsync()
    {
        _sessionState.RestoreSession();
        return Task.CompletedTask;
    }
}
