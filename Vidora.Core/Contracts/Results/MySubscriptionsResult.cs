using System.Collections.Generic;
using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class MySubscriptionsResult
{
    public IReadOnlyList<MySubscription> Subscriptions { get; set; } = [];
}
