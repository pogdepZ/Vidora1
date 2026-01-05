using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class UserProfileResult
{
    public User User { get; set; }
    public Plan CurrentPlan { get; set; }

    public bool HasActivePlan => CurrentPlan != null && !CurrentPlan.IsExpired;
}