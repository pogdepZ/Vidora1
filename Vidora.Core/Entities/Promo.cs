using System;

namespace Vidora.Core.Entities;

public class Promo
{
    public int DiscountId { get; set; }

    public string Code { get; set; } = string.Empty;

    // API: "discountType": "percentage" | "fixed_amount"
    public string DiscountType { get; set; } = string.Empty;

    public decimal Value { get; set; }

    public decimal MinOrderValue { get; set; }

    // 👇 API có thể trả null
    public decimal? MaxDiscount { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsUsed { get; set; }
}
