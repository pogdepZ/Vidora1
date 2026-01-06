using System;

namespace Vidora.Presentation.Gui.Models;

public class Promo
{
    public int DiscountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal MaxDiscount { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsUsed { get; set; }

    public string DisplayText => DiscountType == "percentage" 
        ? $"{Code} - Gi?m {Value}% (t?i ða {MaxDiscount:N0}þ)" 
        : $"{Code} - Gi?m {Value:N0}þ";
}
