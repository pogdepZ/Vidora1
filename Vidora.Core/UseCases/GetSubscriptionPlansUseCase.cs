using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetSubscriptionPlansUseCase
{
    private readonly ISubscriptionApiService _subscriptionApiService;

    public GetSubscriptionPlansUseCase(ISubscriptionApiService subscriptionApiService)
    {
        _subscriptionApiService = subscriptionApiService;
    }

    public Task<Result<SubscriptionPlansResult>> ExecuteAsync()
    {
        return _subscriptionApiService.GetPlansAsync();
    }
}
