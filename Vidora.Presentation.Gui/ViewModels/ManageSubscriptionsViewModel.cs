using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ManageSubscriptionsViewModel : ObservableRecipient, INavigationAware
{
    private readonly ApiClient _apiClient;

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ManageSubscriptionsViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;

        OrderStatusOptions.Add(new FilterOption("All", string.Empty));
        OrderStatusOptions.Add(new FilterOption("PENDING", "PENDING"));
        OrderStatusOptions.Add(new FilterOption("PAID", "PAID"));
        OrderStatusOptions.Add(new FilterOption("COMPLETED", "COMPLETED"));
        OrderStatusOptions.Add(new FilterOption("CANCELLED", "CANCELLED"));
        SelectedOrderStatus = OrderStatusOptions[0];

        PromoTypeOptions.Add(new FilterOption("All", string.Empty));
        PromoTypeOptions.Add(new FilterOption("percentage", "percentage"));
        PromoTypeOptions.Add(new FilterOption("fixed_amount", "fixed_amount"));
        SelectedPromoType = PromoTypeOptions[0];

        PromoActiveOptions.Add(new FilterOption("All", string.Empty));
        PromoActiveOptions.Add(new FilterOption("Active", "true"));
        PromoActiveOptions.Add(new FilterOption("Inactive", "false"));
        SelectedPromoActive = PromoActiveOptions[0];

        ResetAddPromoForm();
    }

    public ObservableCollection<SubscriptionPlanVm> Plans { get; } = new();
    public ObservableCollection<OrderVm> Orders { get; } = new();
    public ObservableCollection<PromoVm> Promos { get; } = new();

    public ObservableCollection<FilterOption> OrderStatusOptions { get; } = new();
    public ObservableCollection<FilterOption> OrderPlanOptions { get; } = new();
    public ObservableCollection<FilterOption> PromoTypeOptions { get; } = new();
    public ObservableCollection<FilterOption> PromoActiveOptions { get; } = new();
    public ObservableCollection<string> PromoCreateTypeOptions { get; } = new() { "percentage", "fixed_amount" };

    [ObservableProperty]
    private FilterOption? selectedOrderStatus;

    [ObservableProperty]
    private FilterOption? selectedOrderPlan;

    [ObservableProperty]
    private FilterOption? selectedPromoType;

    [ObservableProperty]
    private FilterOption? selectedPromoActive;

    [ObservableProperty]
    private string orderSearchText = string.Empty;

    [ObservableProperty]
    private string promoSearchText = string.Empty;

    [ObservableProperty]
    private bool isLoadingPlans;

    [ObservableProperty]
    private bool isLoadingOrders;

    [ObservableProperty]
    private bool isLoadingPromos;

    private int _orderPage = 1;
    private readonly int _orderLimit = 10;
    private int _orderTotalPages = 1;
    private int _orderTotal = 0;

    private int _promoPage = 1;
    private readonly int _promoLimit = 10;
    private int _promoTotalPages = 1;
    private int _promoTotal = 0;

    [ObservableProperty]
    private bool isNotificationOpen;

    [ObservableProperty]
    private InfoBarSeverity notificationSeverity = InfoBarSeverity.Success;

    [ObservableProperty]
    private string notificationMessage = string.Empty;

    [ObservableProperty]
    private bool isAddPromoErrorOpen;

    [ObservableProperty]
    private string addPromoErrorMessage = string.Empty;

    [ObservableProperty]
    private string promoCode = string.Empty;

    [ObservableProperty]
    private string selectedPromoCreateType = "percentage";

    [ObservableProperty]
    private double promoValue = double.NaN;

    [ObservableProperty]
    private double promoMinOrderValue = double.NaN;

    [ObservableProperty]
    private double promoMaxDiscount = double.NaN;

    [ObservableProperty]
    private DateTimeOffset promoStartDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset promoEndDate = DateTimeOffset.Now.AddDays(30);

    public string PlansCountText => Plans.Count.ToString();
    public string OrdersTotalText => _orderTotal.ToString();
    public string PromosTotalText => _promoTotal.ToString();

    public bool OrderCanPrev => _orderPage > 1;
    public bool OrderCanNext => _orderPage < _orderTotalPages;
    public string OrderPageText => $"{_orderPage} / {_orderTotalPages}";

    public bool PromoCanPrev => _promoPage > 1;
    public bool PromoCanNext => _promoPage < _promoTotalPages;
    public string PromoPageText => $"{_promoPage} / {_promoTotalPages}";

    public async Task OnNavigatedToAsync(object parameter)
    {
        SelectedOrderStatus = OrderStatusOptions[0];
        SelectedPromoType = PromoTypeOptions[0];
        SelectedPromoActive = PromoActiveOptions[0];

        await LoadPlansAsync();
        await LoadOrdersAsync(resetPage: true);
        await LoadPromosAsync(resetPage: true);
        UpdateHeaderCounts();
    }

    public async Task OnNavigatedFromAsync()
    {
    }

    [RelayCommand]
    private async Task RefreshPlansAsync()
        => await LoadPlansAsync();

    [RelayCommand]
    private async Task RefreshOrdersAsync()
        => await LoadOrdersAsync(resetPage: false);

    [RelayCommand]
    private async Task ApplyOrderFiltersAsync()
        => await LoadOrdersAsync(resetPage: true);

    [RelayCommand]
    private async Task RefreshPromosAsync()
        => await LoadPromosAsync(resetPage: false);

    [RelayCommand]
    private async Task ApplyPromoFiltersAsync()
        => await LoadPromosAsync(resetPage: true);

    [RelayCommand]
    private async Task ClearOrderFiltersAsync()
    {
        OrderSearchText = string.Empty;
        if (OrderStatusOptions.Count > 0) SelectedOrderStatus = OrderStatusOptions[0];
        if (OrderPlanOptions.Count > 0) SelectedOrderPlan = OrderPlanOptions[0];
        await LoadOrdersAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task ClearPromoFiltersAsync()
    {
        PromoSearchText = string.Empty;
        if (PromoTypeOptions.Count > 0) SelectedPromoType = PromoTypeOptions[0];
        if (PromoActiveOptions.Count > 0) SelectedPromoActive = PromoActiveOptions[0];
        await LoadPromosAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task OrderPrevAsync()
    {
        if (!OrderCanPrev) return;
        _orderPage--;
        await LoadOrdersAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task OrderNextAsync()
    {
        if (!OrderCanNext) return;
        _orderPage++;
        await LoadOrdersAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task PromoPrevAsync()
    {
        if (!PromoCanPrev) return;
        _promoPage--;
        await LoadPromosAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task PromoNextAsync()
    {
        if (!PromoCanNext) return;
        _promoPage++;
        await LoadPromosAsync(resetPage: false);
    }

    public void ResetAddPromoForm()
    {
        IsAddPromoErrorOpen = false;
        AddPromoErrorMessage = string.Empty;

        PromoCode = string.Empty;
        SelectedPromoCreateType = "percentage";

        PromoValue = double.NaN;
        PromoMinOrderValue = double.NaN;
        PromoMaxDiscount = double.NaN;

        PromoStartDate = DateTimeOffset.Now;
        PromoEndDate = DateTimeOffset.Now.AddDays(30);
    }

    public async Task<bool> TryCreatePromoAsync()
    {
        var code = (PromoCode ?? string.Empty).Trim();
        var discountType = (SelectedPromoCreateType ?? "percentage").Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ShowAddPromoError("Code is required.");
            return false;
        }

        if (double.IsNaN(PromoValue) || PromoValue <= 0)
        {
            ShowAddPromoError("Value is required and must be > 0.");
            return false;
        }

        if (discountType == "percentage" && (PromoValue < 1 || PromoValue > 100))
        {
            ShowAddPromoError("Percentage value must be between 1 and 100.");
            return false;
        }

        if (double.IsNaN(PromoMinOrderValue) || PromoMinOrderValue < 0)
        {
            ShowAddPromoError("Min Order Value is required and must be >= 0.");
            return false;
        }

        if (PromoEndDate < PromoStartDate)
        {
            ShowAddPromoError("End Date must be >= Start Date.");
            return false;
        }

        double? maxDiscount = null;
        if (!double.IsNaN(PromoMaxDiscount))
        {
            if (PromoMaxDiscount < 0)
            {
                ShowAddPromoError("Max Discount must be >= 0.");
                return false;
            }
            maxDiscount = PromoMaxDiscount;
        }

        try
        {
            var body = new
            {
                code = code,
                discountType = discountType,
                value = PromoValue,
                minOrderValue = PromoMinOrderValue,
                maxDiscount = maxDiscount,
                startDate = PromoStartDate.UtcDateTime,
                endDate = PromoEndDate.UtcDateTime
            };

            var httpRes = await _apiClient.PostAsync("api/promos/", body);

            if (httpRes.IsSuccessStatusCode)
            {
                ShowToast("Created promo successfully!", InfoBarSeverity.Success);
                await LoadPromosAsync(resetPage: true);
                return true;
            }

            var raw = await httpRes.Content.ReadAsStringAsync();
            ShowAddPromoError($"Create promo failed: {raw}");
            return false;
        }
        catch (Exception ex)
        {
            ShowAddPromoError($"Create promo error: {ex.Message}");
            return false;
        }
    }

    private void UpdateHeaderCounts()
    {
        OnPropertyChanged(nameof(PlansCountText));
        OnPropertyChanged(nameof(OrdersTotalText));
        OnPropertyChanged(nameof(PromosTotalText));
        OnPropertyChanged(nameof(OrderCanPrev));
        OnPropertyChanged(nameof(OrderCanNext));
        OnPropertyChanged(nameof(OrderPageText));
        OnPropertyChanged(nameof(PromoCanPrev));
        OnPropertyChanged(nameof(PromoCanNext));
        OnPropertyChanged(nameof(PromoPageText));
    }

    private void ShowToast(string message, InfoBarSeverity severity = InfoBarSeverity.Success)
    {
        NotificationSeverity = severity;
        NotificationMessage = message;
        IsNotificationOpen = true;
    }

    private void ShowAddPromoError(string message)
    {
        AddPromoErrorMessage = message;
        IsAddPromoErrorOpen = true;
    }

    private async Task LoadPlansAsync()
    {
        try
        {
            IsLoadingPlans = true;

            var httpRes = await _apiClient.GetAsync("api/subscriptions/plans");
            if (!httpRes.IsSuccessStatusCode)
            {
                var raw = await httpRes.Content.ReadAsStringAsync();
                ShowToast($"Load plans failed: {raw}", InfoBarSeverity.Error);
                return;
            }

            var rawJson = await httpRes.Content.ReadAsStringAsync();
            var dto = System.Text.Json.JsonSerializer.Deserialize<SubscriptionsApiListResponse<PlanDto>>(rawJson, JsonOpts);

            Plans.Clear();
            if (dto?.Data != null)
            {
                foreach (var p in dto.Data)
                {
                    Plans.Add(new SubscriptionPlanVm
                    {
                        PlanId = p.PlanId,
                        Name = p.Name ?? string.Empty,
                        Price = p.Price,
                        Durations = p.Durations,
                        Description = p.Description ?? string.Empty
                    });
                }
            }

            OrderPlanOptions.Clear();
            OrderPlanOptions.Add(new FilterOption("All", string.Empty));
            foreach (var p in Plans)
                OrderPlanOptions.Add(new FilterOption(p.Name, p.PlanId.ToString()));
            SelectedOrderPlan = OrderPlanOptions[0];

            UpdateHeaderCounts();
        }
        catch (Exception ex)
        {
            ShowToast($"Load plans error: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            IsLoadingPlans = false;
        }
    }

    private async Task LoadOrdersAsync(bool resetPage)
    {
        if (resetPage) _orderPage = 1;

        try
        {
            IsLoadingOrders = true;

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = _orderPage.ToString();
            qs["limit"] = _orderLimit.ToString();

            var search = (OrderSearchText ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(search)) qs["search"] = search;

            var status = SelectedOrderStatus?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(status)) qs["status"] = status;

            var planId = SelectedOrderPlan?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(planId)) qs["planId"] = planId;

            var url = $"api/orders/all?{qs}";
            var httpRes = await _apiClient.GetAsync(url);

            if (!httpRes.IsSuccessStatusCode)
            {
                var raw = await httpRes.Content.ReadAsStringAsync();
                ShowToast($"Load orders failed: {raw}", InfoBarSeverity.Error);
                return;
            }

            var rawJson = await httpRes.Content.ReadAsStringAsync();
            var dto = System.Text.Json.JsonSerializer.Deserialize<SubscriptionsApiPaginatedResponse<SubscriptionOrderDto>>(rawJson, JsonOpts);

            Orders.Clear();
            if (dto?.Data != null)
            {
                foreach (var o in dto.Data)
                {
                    Orders.Add(new OrderVm
                    {
                        OrderId = o.OrderId,
                        OrderCode = o.OrderCode ?? string.Empty,
                        OrderType = o.OrderType ?? string.Empty,
                        Email = o.Email ?? string.Empty,
                        FullName = o.FullName ?? string.Empty,
                        Status = o.Status ?? string.Empty,
                        PlanName = o.PlanName ?? string.Empty,
                        FinalAmount = o.FinalAmount,
                        PaidAt = o.PaidAt
                    });
                }
            }

            _orderTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            _orderTotalPages = dto?.Pagination?.TotalPages ?? 1;
            if (_orderTotalPages < 1) _orderTotalPages = 1;

            if (_orderPage < 1) _orderPage = 1;
            if (_orderPage > _orderTotalPages) _orderPage = _orderTotalPages;

            UpdateHeaderCounts();
        }
        catch (Exception ex)
        {
            ShowToast($"Load orders error: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            IsLoadingOrders = false;
        }
    }

    private async Task LoadPromosAsync(bool resetPage)
    {
        if (resetPage) _promoPage = 1;

        try
        {
            IsLoadingPromos = true;

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = _promoPage.ToString();
            qs["limit"] = _promoLimit.ToString();

            var search = (PromoSearchText ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(search)) qs["search"] = search;

            var type = SelectedPromoType?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(type)) qs["discountType"] = type;

            var active = SelectedPromoActive?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(active)) qs["active"] = active;

            var url = $"api/promos/?{qs}";
            var httpRes = await _apiClient.GetAsync(url);

            if (!httpRes.IsSuccessStatusCode)
            {
                var raw = await httpRes.Content.ReadAsStringAsync();
                ShowToast($"Load promos failed: {raw}", InfoBarSeverity.Error);
                return;
            }

            var rawJson = await httpRes.Content.ReadAsStringAsync();
            var dto = System.Text.Json.JsonSerializer.Deserialize<SubscriptionsApiPaginatedResponse<PromoDto>>(rawJson, JsonOpts);

            Promos.Clear();
            if (dto?.Data != null)
            {
                foreach (var p in dto.Data)
                {
                    Promos.Add(new PromoVm
                    {
                        DiscountId = p.DiscountId,
                        Code = p.Code ?? string.Empty,
                        DiscountType = p.DiscountType ?? string.Empty,
                        Value = p.Value,
                        MinOrderValue = p.MinOrderValue,
                        MaxDiscount = p.MaxDiscount,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate
                    });
                }
            }

            _promoTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            _promoTotalPages = dto?.Pagination?.TotalPages ?? 1;
            if (_promoTotalPages < 1) _promoTotalPages = 1;

            if (_promoPage < 1) _promoPage = 1;
            if (_promoPage > _promoTotalPages) _promoPage = _promoTotalPages;

            UpdateHeaderCounts();
        }
        catch (Exception ex)
        {
            ShowToast($"Load promos error: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            IsLoadingPromos = false;
        }
    }
}

public sealed class FilterOption
{
    public FilterOption(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }
    public string Value { get; }

    public override string ToString() => Label;
}

public sealed class SubscriptionPlanVm
{
    public int PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Durations { get; set; }
    public string Description { get; set; } = string.Empty;

    public string PriceDisplay => $"{Price:N0} đ";
    public string DurationDisplay => $"{Durations} days";
}

public sealed class OrderVm
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int FinalAmount { get; set; }
    public DateTime? PaidAt { get; set; }

    public string FinalAmountDisplay => $"{FinalAmount:N0} đ";
    public string PaidAtDisplay => PaidAt.HasValue ? PaidAt.Value.ToString("yyyy-MM-dd") : "-";
}

public sealed class PromoVm
{
    public int DiscountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public int Value { get; set; }
    public int MinOrderValue { get; set; }
    public int? MaxDiscount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string ValueDisplay => DiscountType == "percentage" ? $"{Value}%" : $"{Value:N0} đ";
    public string MinOrderValueDisplay => $"{MinOrderValue:N0} đ";
    public string MaxDiscountDisplay => MaxDiscount.HasValue ? $"{MaxDiscount.Value:N0} đ" : "-";
    public string DateRangeDisplay => $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}";
}

public sealed class SubscriptionsApiListResponse<T>
{
    public bool Success { get; set; }
    public T[]? Data { get; set; }
}

public sealed class SubscriptionsApiPaginatedResponse<T>
{
    public bool Success { get; set; }
    public T[]? Data { get; set; }
    public SubscriptionsPaginationDto? Pagination { get; set; }
}

public sealed class SubscriptionsPaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

public sealed class PlanDto
{
    public int PlanId { get; set; }
    public string? Name { get; set; }
    public int Price { get; set; }
    public int Durations { get; set; }
    public string? Description { get; set; }
}

public sealed class SubscriptionOrderDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? OrderCode { get; set; }
    public string? OrderType { get; set; }
    public string? Status { get; set; }
    public int Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public int PlanId { get; set; }
    public string? PlanName { get; set; }
    public int FinalAmount { get; set; }
}

public sealed class PromoDto
{
    public int DiscountId { get; set; }
    public string? Code { get; set; }
    public string? DiscountType { get; set; }
    public int Value { get; set; }
    public int MinOrderValue { get; set; }
    public int? MaxDiscount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
