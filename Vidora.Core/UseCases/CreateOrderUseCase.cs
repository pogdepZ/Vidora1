using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class CreateOrderUseCase
{
    private readonly IOrderApiService _orderApiService;

    public CreateOrderUseCase(IOrderApiService orderApiService)
    {
        _orderApiService = orderApiService;
    }

    public Task<Result<CreateOrderResult>> ExecuteAsync(int planId)
    {
        return _orderApiService.CreateOrderAsync(planId);
    }
}
