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

    public async Task<Result<MySubscriptionsResult>> GetMySubscriptionsAsync()
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<MySubscriptionsResult>("Access token not found. Please login again.");
        }

        var url = "api/subscriptions/my";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        var apiRes = await httpRes.ReadListAsync<MySubscriptionData>();

        if (apiRes is not ListSuccessResponse<MySubscriptionData> success)
        {
            return Result.Failure<MySubscriptionsResult>(
                apiRes.Message ?? "Lấy danh sách gói đi mua"
            );
        }

        var result = new MySubscriptionsResult
        {
            Subscriptions = _mapper.Map<IReadOnlyList<MySubscription>>(success.Data)
        };

        return Result.Success(result);
    }
}
