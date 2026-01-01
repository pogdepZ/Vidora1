using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;

namespace Vidora.Core.UseCases;

public class LogoutUseCase
{
    private readonly ISessionStateService _sessionStateService;

    public LogoutUseCase(ISessionStateService sessionStateService)
    {
        _sessionStateService = sessionStateService;
    }

    public async Task<Result> ExecuteAsync()
    {
        try
        {
            return await ExecuteAsyncInternal();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Logout failed: {ex.Message}");
        }
    }

    private async Task<Result> ExecuteAsyncInternal()
    {
        _sessionStateService.ClearSession();

        return Result.Success();
    }
}
