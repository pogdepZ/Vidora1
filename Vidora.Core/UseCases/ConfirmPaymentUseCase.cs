using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class ConfirmPaymentUseCase
{
    private readonly IOrderApiService _orderApiService;

    public ConfirmPaymentUseCase(IOrderApiService orderApiService)
    {
        _orderApiService = orderApiService;
    }

    public Task<Result<ConfirmPaymentResult>> ExecuteAsync(int orderId)
    {
        return _orderApiService.ConfirmPaymentAsync(orderId);
    }
}
