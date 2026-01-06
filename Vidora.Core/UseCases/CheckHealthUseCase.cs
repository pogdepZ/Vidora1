using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Interfaces.Api;
namespace Vidora.Core.UseCases;

public class CheckHealthUseCase
{
    private readonly IHealthApiService _healthApiService;
    public CheckHealthUseCase(IHealthApiService healthApiService)
    {
        _healthApiService = healthApiService;
    }

    public async Task<Result> ExecuteAsync()
    {
        try
        {
            return await ExecuteAsyncInternal();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Cannot connect to server: {ex.Message}");
        }
    }

    private async Task<Result> ExecuteAsyncInternal()
    {
        var apiRes = await _healthApiService.CheckHealthAsync();

        return apiRes;
    }
}
