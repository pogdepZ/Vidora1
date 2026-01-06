using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface IOrderApiService
{
    Task<Result<CreateOrderResult>> CreateOrderAsync(int planId);
    Task<Result<AvailablePromosResult>> GetAvailablePromosAsync();
    Task<Result<ApplyDiscountResult>> ApplyDiscountAsync(int orderId, int discountId);
    Task<Result<ConfirmPaymentResult>> ConfirmPaymentAsync(int orderId);
}
