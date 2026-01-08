using System;

namespace Vidora.Presentation.Gui.Models;

public class Promo
{
    public int DiscountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsUsed { get; set; }

    public string DisplayText => DiscountType == "percentage" 
        ? $"{Code} - Giảm {Value}% (tối đa {MaxDiscount:N0}₫)" 
        : $"{Code} - Giảm {Value:N0}₫";
}
    