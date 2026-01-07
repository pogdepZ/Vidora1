using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetCurrentSubscriptionUseCase
{
    private readonly ISubscriptionApiService _subscriptionApiService;

    public GetCurrentSubscriptionUseCase(ISubscriptionApiService subscriptionApiService)
    {
        _subscriptionApiService = subscriptionApiService;
    }

    public Task<Result<CurrentSubscriptionResult>> ExecuteAsync()
    {
        return _subscriptionApiService.GetCurrentSubscriptionAsync();
    }
}
