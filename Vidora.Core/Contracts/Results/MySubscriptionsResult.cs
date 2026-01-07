using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class CurrentSubscriptionResult
{
    public CurrentSubscription? Subscription { get; set; }
    public bool HasActiveSubscription => Subscription?.IsActive == true;
}
