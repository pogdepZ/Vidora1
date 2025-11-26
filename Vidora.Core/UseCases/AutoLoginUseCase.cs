using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Interfaces.Storage;

namespace Vidora.Core.UseCases;

public class AutoLoginUseCase
{
    private readonly ISessionStateService _sessionState;
    private readonly ISessionStorageService _sessionStorage;

    public AutoLoginUseCase(ISessionStateService sessionState, ISessionStorageService sessionStorage)
    {
        _sessionState = sessionState; 
        _sessionStorage = sessionStorage;
    }

    public async Task<Result> ExecuteAsync()
    {
        _sessionState.RestoreSession();
        if (_sessionState.IsSessionValid)
        {
            return Result.Success();
        }

        return Result.Failure("No valid session found. Please log in.");
    }
}
