using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Vidora.Infrastructure.Api.Clients;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageSubscriptionsPage : Page
{
    private readonly ApiClient _apiClient;

    private static readonly System.Text.Json.JsonSerializerOptions _jsonOpts =
   new() { PropertyNameCaseInsensitive = true };

    public ManageSubscriptionsPage()
    {
        InitializeComponent();

        _apiClient = App.GetService<ApiClient>();

        Plans = new ObservableCollection<SubscriptionPlanVm>();
        Orders = new ObservableCollection<OrderVm>();
        Promos = new ObservableCollection<PromoVm>();

        Loaded += ManageSubscriptionsPage_Loaded;
    }

    private async void ManageSubscriptionsPage_Loaded(object sender, RoutedEventArgs e)
    {
        // defaults UI
        OrderStatusComboBox.SelectedIndex = 0;
        PromoTypeFilterComboBox.SelectedIndex = 0;
        PromoActiveFilterComboBox.SelectedIndex = 0;

        await LoadPlansAsync();
        await LoadOrdersAsync(resetPage: true);
        await LoadPromosAsync(resetPage: true);
        UpdateHeaderCounts();
    }

    // ===================== BINDABLE PROPS (code-behind) =====================
    public ObservableCollection<SubscriptionPlanVm> Plans { get; }
    public ObservableCollection<OrderVm> Orders { get; }
    public ObservableCollection<PromoVm> Promos { get; }

    public bool IsLoadingPlans { get; set; }
    public bool IsLoadingOrders { get; set; }
    public bool IsLoadingPromos { get; set; }

    // pagination state (Orders)
    private int _orderPage = 1;
    private int _orderLimit = 10;
    private int _orderTotalPages = 1;
    private int _orderTotal = 0;

    // pagination state (Promos)
    private int _promoPage = 1;
    private int _promoLimit = 10;
    private int _promoTotalPages = 1;
    private int _promoTotal = 0;

    public string PlansCountText => Plans.Count.ToString();
    public string OrdersTotalText => _orderTotal.ToString();
    public string PromosTotalText => _promoTotal.ToString();

    public bool OrderCanPrev => _orderPage > 1;
    public bool OrderCanNext => _orderPage < _orderTotalPages;
    public string OrderPageText => $"{_orderPage} / {_orderTotalPages}";

    public bool PromoCanPrev => _promoPage > 1;
    public bool PromoCanNext => _promoPage < _promoTotalPages;
    public string PromoPageText => $"{_promoPage} / {_promoTotalPages}";

    private void UpdateHeaderCounts()
    {
        // force UI update for x:Bind OneWay props
        Bindings.Update();
    }

    private void ShowToast(string message, InfoBarSeverity severity = InfoBarSeverity.Success)
    {
        NotificationInfoBar.Severity = severity;
        NotificationInfoBar.Message = message;
        NotificationInfoBar.IsOpen = true;
    }

    private static string? ComboTag(ComboBox cb)
        => (cb.SelectedItem as ComboBoxItem)?.Tag?.ToString();

    // ===================== LOAD PLANS =====================
    private async Task LoadPlansAsync()
    {
        try
        {
            IsLoadingPlans = true;
            Bindings.Update();

            var httpRes = await _apiClient.GetAsync("api/subscriptions/plans");
            if (!httpRes.IsSuccessStatusCode)
            {
                var raw = await httpRes.Content.ReadAsStringAsync();
                ShowToast($"Load plans failed: {raw}", InfoBarSeverity.Error);
                return;
            }
           
    var rawJson = await httpRes.Content.ReadAsStringAsync();
            var dto = System.Text.Json.JsonSerializer.Deserialize<ApiListResponse<PlanDto>>(rawJson, _jsonOpts);


            Plans.Clear();
            if (dto?.Data != null)
            {
                foreach (var p in dto.Data)
                {
                    Plans.Add(new SubscriptionPlanVm
                    {
                        PlanId = p.PlanId,
                        Name = p.Name ?? "",
                        Price = p.Price,
                        Durations = p.Durations,
                        Description = p.Description ?? ""
                    });
                }
            }

            // Fill OrderPlanComboBox (All + plans)
            OrderPlanComboBox.Items.Clear();
            OrderPlanComboBox.Items.Add(new ComboBoxItem { Content = "All", Tag = "" });
            foreach (var p in Plans)
                OrderPlanComboBox.Items.Add(new ComboBoxItem { Content = p.Name, Tag = p.PlanId.ToString() });
            OrderPlanComboBox.SelectedIndex = 0;

            UpdateHeaderCounts();
        }
        catch (Exception ex)
        {
            ShowToast($"Load plans error: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            IsLoadingPlans = false;
            Bindings.Update();
        }
    }

    private async void OnRefreshPlansClick(object sender, RoutedEventArgs e)
        => await LoadPlansAsync();

    // ===================== LOAD ORDERS =====================
    private async Task LoadOrdersAsync(bool resetPage)
    {
        if (resetPage) _orderPage = 1;

        try
        {
            IsLoadingOrders = true;
            Bindings.Update();

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = _orderPage.ToString();
            qs["limit"] = _orderLimit.ToString();

            var search = (OrderSearchTextBox.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(search)) qs["search"] = search;

            var status = ComboTag(OrderStatusComboBox);
            if (!string.IsNullOrWhiteSpace(status)) qs["status"] = status;

            var planId = ComboTag(OrderPlanComboBox);
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

            var dto = System.Text.Json.JsonSerializer.Deserialize<ApiPaginatedResponse<OrderDto>>(rawJson, _jsonOpts);


            Orders.Clear();
            if (dto?.Data != null)
            {
                foreach (var o in dto.Data)
                {
                    Orders.Add(new OrderVm
                    {
                        OrderId = o.OrderId,
                        OrderCode = o.OrderCode ?? "",
                        OrderType = o.OrderType ?? "",
                        Email = o.Email ?? "",
                        FullName = o.FullName ?? "",
                        Status = o.Status ?? "",
                        PlanName = o.PlanName ?? "",
                        FinalAmount = o.FinalAmount,
                        PaidAt = o.PaidAt
                    });
                }
            }

            _orderTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            _orderTotalPages = dto?.Pagination?.TotalPages ?? 1;
            if (_orderTotalPages < 1) _orderTotalPages = 1;

            // clamp page
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
            Bindings.Update();
        }
    }

    private async void OnRefreshOrdersClick(object sender, RoutedEventArgs e)
        => await LoadOrdersAsync(resetPage: false);

    private async void OnOrderSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await LoadOrdersAsync(resetPage: true);
    }

    private async void OnOrderFilterChanged(object sender, SelectionChangedEventArgs e)
        => await LoadOrdersAsync(resetPage: true);

    private async void OnClearOrderFiltersClick(object sender, RoutedEventArgs e)
    {
        OrderSearchTextBox.Text = "";
        OrderStatusComboBox.SelectedIndex = 0;
        if (OrderPlanComboBox.Items.Count > 0) OrderPlanComboBox.SelectedIndex = 0;
        await LoadOrdersAsync(resetPage: true);
    }

    private async void OnOrderPrevClick(object sender, RoutedEventArgs e)
    {
        if (!OrderCanPrev) return;
        _orderPage--;
        await LoadOrdersAsync(resetPage: false);
    }

    private async void OnOrderNextClick(object sender, RoutedEventArgs e)
    {
        if (!OrderCanNext) return;
        _orderPage++;
        await LoadOrdersAsync(resetPage: false);
    }

    // ===================== LOAD PROMOS =====================
    private async Task LoadPromosAsync(bool resetPage)
    {
        if (resetPage) _promoPage = 1;

        try
        {
            IsLoadingPromos = true;
            Bindings.Update();

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = _promoPage.ToString();
            qs["limit"] = _promoLimit.ToString();

            var search = (PromoSearchTextBox.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(search)) qs["search"] = search;

            var type = ComboTag(PromoTypeFilterComboBox);
            if (!string.IsNullOrWhiteSpace(type)) qs["discountType"] = type;

            var active = ComboTag(PromoActiveFilterComboBox);
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
            var dto = System.Text.Json.JsonSerializer.Deserialize<ApiPaginatedResponse<PromoDto>>(rawJson, _jsonOpts);


            Promos.Clear();
            if (dto?.Data != null)
            {
                foreach (var p in dto.Data)
                {
                    Promos.Add(new PromoVm
                    {
                        DiscountId = p.DiscountId,
                        Code = p.Code ?? "",
                        DiscountType = p.DiscountType ?? "",
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

            // clamp page
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
            Bindings.Update();
        }
    }

    private async void OnRefreshPromosClick(object sender, RoutedEventArgs e)
        => await LoadPromosAsync(resetPage: false);

    private async void OnPromoSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await LoadPromosAsync(resetPage: true);
    }

    private async void OnPromoFilterChanged(object sender, SelectionChangedEventArgs e)
        => await LoadPromosAsync(resetPage: true);

    private async void OnClearPromoFiltersClick(object sender, RoutedEventArgs e)
    {
        PromoSearchTextBox.Text = "";
        PromoTypeFilterComboBox.SelectedIndex = 0;
        PromoActiveFilterComboBox.SelectedIndex = 0;
        await LoadPromosAsync(resetPage: true);
    }

    private async void OnPromoPrevClick(object sender, RoutedEventArgs e)
    {
        if (!PromoCanPrev) return;
        _promoPage--;
        await LoadPromosAsync(resetPage: false);
    }

    private async void OnPromoNextClick(object sender, RoutedEventArgs e)
    {
        if (!PromoCanNext) return;
        _promoPage++;
        await LoadPromosAsync(resetPage: false);
    }

    // ===================== ADD PROMO (POPUP) =====================
    private void ResetAddPromoForm()
    {
        AddPromoInfoBar.IsOpen = false;
        AddPromoInfoBar.Message = "";

        PromoCodeTextBox.Text = "";
        PromoTypeComboBox.SelectedIndex = 0;

        PromoValueNumberBox.Value = double.NaN;
        PromoMinOrderNumberBox.Value = double.NaN;
        PromoMaxDiscountNumberBox.Value = double.NaN;

        PromoStartDatePicker.Date = DateTimeOffset.Now;
        PromoEndDatePicker.Date = DateTimeOffset.Now.AddDays(30);
    }

    private async void OnAddPromoClick(object sender, RoutedEventArgs e)
    {
        ResetAddPromoForm();
        AddPromoDialog.XamlRoot = this.XamlRoot;
        await AddPromoDialog.ShowAsync();
    }

    private async void OnAddPromoPrimaryClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true; // giữ dialog nếu lỗi

        var code = (PromoCodeTextBox.Text ?? "").Trim();
        var discountType = ((PromoTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "percentage").Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ShowAddPromoError("Code is required.");
            return;
        }

        if (double.IsNaN(PromoValueNumberBox.Value) || PromoValueNumberBox.Value <= 0)
        {
            ShowAddPromoError("Value is required and must be > 0.");
            return;
        }

        if (discountType == "percentage" && (PromoValueNumberBox.Value < 1 || PromoValueNumberBox.Value > 100))
        {
            ShowAddPromoError("Percentage value must be between 1 and 100.");
            return;
        }

        if (double.IsNaN(PromoMinOrderNumberBox.Value) || PromoMinOrderNumberBox.Value < 0)
        {
            ShowAddPromoError("Min Order Value is required and must be >= 0.");
            return;
        }

        var start = PromoStartDatePicker.Date;
        var end = PromoEndDatePicker.Date;
        if (end < start)
        {
            ShowAddPromoError("End Date must be >= Start Date.");
            return;
        }

        double? maxDiscount = null;
        if (!double.IsNaN(PromoMaxDiscountNumberBox.Value))
        {
            if (PromoMaxDiscountNumberBox.Value < 0)
            {
                ShowAddPromoError("Max Discount must be >= 0.");
                return;
            }
            maxDiscount = PromoMaxDiscountNumberBox.Value;
        }

        try
        {
            sender.IsPrimaryButtonEnabled = false;
            sender.IsSecondaryButtonEnabled = false;

            var body = new
            {
                code = code,
                discountType = discountType,
                value = PromoValueNumberBox.Value,
                minOrderValue = PromoMinOrderNumberBox.Value,
                maxDiscount = maxDiscount,
                startDate = start.UtcDateTime,
                endDate = end.UtcDateTime
            };

            var httpRes = await _apiClient.PostAsync("api/promos/", body);

            if (httpRes.IsSuccessStatusCode)
            {
                sender.Hide();
                ShowToast("Created promo successfully!", InfoBarSeverity.Success);

                await LoadPromosAsync(resetPage: true);
                return;
            }

            var raw = await httpRes.Content.ReadAsStringAsync();
            ShowAddPromoError($"Create promo failed: {raw}");
        }
        catch (Exception ex)
        {
            ShowAddPromoError($"Create promo error: {ex.Message}");
        }
        finally
        {
            sender.IsPrimaryButtonEnabled = true;
            sender.IsSecondaryButtonEnabled = true;
        }
    }

    private void ShowAddPromoError(string message)
    {
        AddPromoInfoBar.Severity = InfoBarSeverity.Error;
        AddPromoInfoBar.Message = message;
        AddPromoInfoBar.IsOpen = true;
    }
}

/* ===================== SIMPLE DTO/VM ===================== */

public sealed class SubscriptionPlanVm
{
    public int PlanId { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public int Durations { get; set; }
    public string Description { get; set; } = "";

    public string PriceDisplay => $"{Price:N0} đ";
    public string DurationDisplay => $"{Durations} days";
}

public sealed class OrderVm
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = "";
    public string OrderType { get; set; } = ""; // NEW
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Status { get; set; } = "";
    public string PlanName { get; set; } = "";
    public int FinalAmount { get; set; }
    public DateTime? PaidAt { get; set; }

    public string FinalAmountDisplay => $"{FinalAmount:N0} đ";
    public string PaidAtDisplay => PaidAt.HasValue ? PaidAt.Value.ToString("yyyy-MM-dd") : "-";
}

public sealed class PromoVm
{
    public int DiscountId { get; set; }
    public string Code { get; set; } = "";
    public string DiscountType { get; set; } = "";
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

/* ====== API RESPONSE WRAPPER ====== */

public sealed class ApiListResponse<T>
{
    public bool Success { get; set; }
    public T[]? Data { get; set; }
}

public sealed class ApiPaginatedResponse<T>
{
    public bool Success { get; set; }
    public T[]? Data { get; set; }
    public PaginationDto? Pagination { get; set; }
}

public sealed class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

/* ====== DTO match server ====== */

public sealed class PlanDto
{
    public int PlanId { get; set; }
    public string? Name { get; set; }
    public int Price { get; set; }
    public int Durations { get; set; }
    public string? Description { get; set; }
}

public sealed class OrderDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? OrderCode { get; set; }
    public string? OrderType { get; set; } // NEW (server có field này)
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
