namespace Vidora.Core.Entities;

public class Order
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;

    public string OrderType { get; set; } = string.Empty;

    public int OriginalAmount { get; set; }

    public int DiscountAmount { get; set; }

    public int FinalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? DiscountId { get; set; }
    public string? DiscountCode { get; set; }
}
