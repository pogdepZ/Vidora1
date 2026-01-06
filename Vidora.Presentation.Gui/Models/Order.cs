namespace Vidora.Presentation.Gui.Models;

public class Order
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? DiscountId { get; set; }
    public string? DiscountCode { get; set; }
}
