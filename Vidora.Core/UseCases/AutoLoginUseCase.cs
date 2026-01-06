using CSharpFunctionalExtensions;
using System;
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

    public async Task<Result> ExecuteAsync()
    {
        try
        {
            return await ExecuteAsyncInternal();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteAsyncInternal()
    {
        _sessionState.RestoreSession();
        if (!_sessionState.IsAuthenticated)
        {
            return Result.Failure("No valid session found. Please log in.");
        }
        
        return Result.Success("Auto login successful.");
    }
}
