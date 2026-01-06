namespace Vidora.Core.Entities;

public class Order
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; } // Chuyển sang decimal
    public decimal DiscountAmount { get; set; } // Chuyển sang decimal
    public decimal FinalAmount { get; set; }    // Chuyển sang decimal
    public string Status { get; set; } = string.Empty;
    public int? DiscountId { get; set; }
    public string? DiscountCode { get; set; }
}