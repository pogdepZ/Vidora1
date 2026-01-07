using AutoMapper;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class OrderApiService : IOrderApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;

    public OrderApiService(ApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Result<CreateOrderResult>> CreateOrderAsync(int planId)
    {
        var url = "api/orders";
        var body = new { planId };


        Debug.WriteLine(planId);
        var httpRes = await _apiClient.PostAsync(url, body);

        
        var apiRes = await httpRes.ReadAsync<OrderData>();

        if (apiRes is not SuccessResponse<OrderData> success)
        {
            return Result.Failure<CreateOrderResult>(
                apiRes.Message ?? "Tạo đơn hàng thất bại"
            );
        }

        var result = new CreateOrderResult
        {
            Order = _mapper.Map<Order>(success.Data)
        };

        return Result.Success(result);
    }

    public async Task<Result<AvailablePromosResult>> GetAvailablePromosAsync()
    {
        var url = "api/promos/available";

        var httpRes = await _apiClient.GetAsync(url);

        var apiRes = await httpRes.ReadListAsync<PromoData>();

        if (apiRes is not ListSuccessResponse<PromoData> success)
        {
            return Result.Failure<AvailablePromosResult>(
                apiRes.Message ?? "Lấy danh sách mã giảm giá thất bại"
            );
        }

        var result = new AvailablePromosResult
        {
            Promos = _mapper.Map<IReadOnlyList<Promo>>(success.Data)
        };

        return Result.Success(result);
    }

    public async Task<Result<ApplyDiscountResult>> ApplyDiscountAsync(int orderId, int discountId)
    {
        var url = "api/orders/apply-discount";
        var body = new { orderId , discountId };


        var httpRes = await _apiClient.PutAsync(url, body);

        var apiRes = await httpRes.ReadAsync<OrderData>();

        if (apiRes is not SuccessResponse<OrderData> success)
        {
            return Result.Failure<ApplyDiscountResult>(
                apiRes.Message ?? "Áp dụng mã giảm giá thất bại"
            );
        }

        var result = new ApplyDiscountResult
        {
            Order = _mapper.Map<Order>(success.Data)
        };

        return Result.Success(result);
    }

    public async Task<Result<ConfirmPaymentResult>> ConfirmPaymentAsync(int orderId)
    {
        var url = $"api/orders/{orderId}/status";
        var body = new { status = "COMPLETED" };

        var httpRes = await _apiClient.PutAsync(url, body);

        if (!httpRes.IsSuccessStatusCode)
        {
            return Result.Failure<ConfirmPaymentResult>("Xác nhận thanh toán thất bại");
        }

        var result = new ConfirmPaymentResult
        {
            Success = true,
            Message = "Thanh toán thành công"
        };

        return Result.Success(result);
    }
}
