using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class UserProfileResult
{
    public required User User { get; set; }
    public Plan? CurrentPlan { get; set; }

    public bool HasActivePlan => CurrentPlan != null && !CurrentPlan.IsExpired;
}