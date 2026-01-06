using AutoMapper;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class OrderApiService : IOrderApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly ISessionStateService _sessionService;

    public OrderApiService(ApiClient apiClient, IMapper mapper, ISessionStateService sessionService)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _sessionService = sessionService;
    }

    public async Task<Result<CreateOrderResult>> CreateOrderAsync(int planId)
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<CreateOrderResult>("Access token not found. Please login again.");
        }
        
        var url = "api/orders";
        var body = new { planId };


        Debug.WriteLine(planId);
        Debug.WriteLine(accessToken);
        var httpRes = await _apiClient.PostAsync(url, body, token: accessToken);

        

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
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<AvailablePromosResult>("Access token not found. Please login again.");
        }

        var url = "api/promos/available";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

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
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<ApplyDiscountResult>("Access token not found. Please login again.");
        }

        var url = "api/orders/apply-discount";
        var body = new { orderId , discountId };

        var httpRes = await _apiClient.PutAsync(url, body, token: accessToken);

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
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<ConfirmPaymentResult>("Access token not found. Please login again.");
        }

        var url = $"api/orders/{orderId}/status";
        var body = new { status = "PAID" };

        var httpRes = await _apiClient.PutAsync(url, body, token: accessToken);

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
