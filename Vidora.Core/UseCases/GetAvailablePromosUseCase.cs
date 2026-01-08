using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetAvailablePromosUseCase
{
    private readonly IOrderApiService _orderApiService;

    public GetAvailablePromosUseCase(IOrderApiService orderApiService)
    {
        _orderApiService = orderApiService;
    }

    public Task<Result<AvailablePromosResult>> ExecuteAsync()
    {
        return _orderApiService.GetAvailablePromosAsync();
    }
}
