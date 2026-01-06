using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetMySubscriptionsUseCase
{
    private readonly ISubscriptionApiService _subscriptionApiService;

    public GetMySubscriptionsUseCase(ISubscriptionApiService subscriptionApiService)
    {
        _subscriptionApiService = subscriptionApiService;
    }

    public Task<Result<MySubscriptionsResult>> ExecuteAsync()
    {
        return _subscriptionApiService.GetMySubscriptionsAsync();
    }
}
