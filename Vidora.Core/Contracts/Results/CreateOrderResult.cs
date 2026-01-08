using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class CreateOrderResult
{
    public Order Order { get; set; } = null!;
}
