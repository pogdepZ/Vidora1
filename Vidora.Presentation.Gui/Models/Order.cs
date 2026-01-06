namespace Vidora.Presentation.Gui.Models;

public class Order
{
    public int OrderId { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? DiscountId { get; set; }
}
