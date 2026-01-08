using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class ApplyDiscountResult
{
    public Order Order { get; set; } = null!;
}
