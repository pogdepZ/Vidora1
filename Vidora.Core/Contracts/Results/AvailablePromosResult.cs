using System.Collections.Generic;
using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class AvailablePromosResult
{
    public IReadOnlyList<Promo> Promos { get; set; } = [];
}
