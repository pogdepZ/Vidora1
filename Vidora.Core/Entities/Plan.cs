using System;

namespace Vidora.Core.Entities;

public class Plan
{
    public int PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int LeftDay { get; set; } 
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public string StatusLabel
    {
        get
        {
            if (IsExpired) return "Đã hết hạn";
            if (LeftDay <= 3) return "Sắp hết hạn";
            return "Đang hoạt động";
        }
    }

    // Logic kiểm tra xem có phải gói VIP không (ví dụ)
    public bool IsPremium => Price > 0;
}