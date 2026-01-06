using System.Collections.Generic;
using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class SubscriptionPlansResult
{
    public IReadOnlyList<SubscriptionPlan> Plans { get; set; } = [];
}
