namespace Vidora.Presentation.Gui.Models;

public class SubscriptionPlan
{
    public int PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Durations { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPurchased { get; set; }
}
