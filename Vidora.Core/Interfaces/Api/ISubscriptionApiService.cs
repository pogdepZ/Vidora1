using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface ISubscriptionApiService
{
    Task<Result<SubscriptionPlansResult>> GetPlansAsync();
    Task<Result<MySubscriptionsResult>> GetMySubscriptionsAsync();
}
