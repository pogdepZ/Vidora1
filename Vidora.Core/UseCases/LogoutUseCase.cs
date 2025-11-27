using CSharpFunctionalExtensions;
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
        _sessionStateService.ClearSession();

        return Result.Success();
    }
}
