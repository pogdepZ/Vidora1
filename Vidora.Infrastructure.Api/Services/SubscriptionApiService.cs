using AutoMapper;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class SubscriptionApiService : ISubscriptionApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly ISessionStateService _sessionService;

    public SubscriptionApiService(ApiClient apiClient, IMapper mapper, ISessionStateService sessionService)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _sessionService = sessionService;
    }

    public async Task<Result<SubscriptionPlansResult>> GetPlansAsync()
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<SubscriptionPlansResult>("Access token not found. Please login again.");
        }

        var url = "api/subscriptions/plans";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        var apiRes = await httpRes.ReadListAsync<SubscriptionPlanData>();

        if (apiRes is not ListSuccessResponse<SubscriptionPlanData> success)
        {
            return Result.Failure<SubscriptionPlansResult>(
                apiRes.Message ?? "Lấy danh sách gói đăng ký thất bại"
            );
        }

        var result = new SubscriptionPlansResult
        {
            Plans = _mapper.Map<IReadOnlyList<SubscriptionPlan>>(success.Data)
        };

        return Result.Success(result);
    }

    public async Task<Result<CurrentSubscriptionResult>> GetCurrentSubscriptionAsync()
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<CurrentSubscriptionResult>("Access token not found. Please login again.");
        }

        var url = "api/subscriptions/current";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        // If no subscription found (404 or empty), return empty result
        if (!httpRes.IsSuccessStatusCode)
        {
            return Result.Success(new CurrentSubscriptionResult { Subscription = null });
        }

        var apiRes = await httpRes.ReadAsync<CurrentSubscriptionData>();

        if (apiRes is not SuccessResponse<CurrentSubscriptionData> success || success.Data == null)
        {
            // No active subscription
            return Result.Success(new CurrentSubscriptionResult { Subscription = null });
        }

        var result = new CurrentSubscriptionResult
        {
            Subscription = _mapper.Map<CurrentSubscription>(success.Data)
        };

        return Result.Success(result);
    }
}
