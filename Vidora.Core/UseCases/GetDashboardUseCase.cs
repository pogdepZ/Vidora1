using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetDashboardUseCase
{
    private readonly IStatsApiService _statsApiService;

    public GetDashboardUseCase(IStatsApiService statsApiService)
    {
        _statsApiService = statsApiService;
    }

    public async Task<Result<DashboardResult>> ExecuteAsync()
    {
        return await _statsApiService.GetDashboardAsync();
    }
}
