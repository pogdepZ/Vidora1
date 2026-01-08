using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class ApplyDiscountUseCase
{
    private readonly IOrderApiService _orderApiService;

    public ApplyDiscountUseCase(IOrderApiService orderApiService)
    {
        _orderApiService = orderApiService;
    }

    public Task<Result<ApplyDiscountResult>> ExecuteAsync(int orderId, int discountId)
    {
        return _orderApiService.ApplyDiscountAsync(orderId, discountId);
    }
}
