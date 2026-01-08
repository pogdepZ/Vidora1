using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ManageSubscriptionsViewModel : ObservableRecipient, INavigationAware
{
    private readonly ApiClient _apiClient;

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ManageSubscriptionsViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;

        Plans = new ObservableCollection<SubscriptionPlanVm>();
        Orders = new ObservableCollection<OrderVm>();
        Promos = new ObservableCollection<PromoVm>();

        Plans.CollectionChanged += OnPlansChanged;
        Orders.CollectionChanged += OnOrdersChanged;
        Promos.CollectionChanged += OnPromosChanged;

        OrderStatusOptions = new ObservableCollection<FilterOption>
        {
            new("All", ""),
            new("PENDING", "PENDING"),
            new("PAID", "PAID"),
            new("COMPLETED", "COMPLETED"),
            new("CANCELLED", "CANCELLED")
        };

        PromoTypeOptions = new ObservableCollection<FilterOption>
        {
            new("All", ""),
            new("percentage", "percentage"),
            new("fixed_amount", "fixed_amount")
        };

        PromoActiveOptions = new ObservableCollection<FilterOption>
        {
            new("All", ""),
            new("Active", "true"),
            new("Inactive", "false")
        };

        OrderPlanOptions = new ObservableCollection<FilterOption>
        {
            new("All", "")
        };

        OrderStatusFilter = string.Empty;
        PromoTypeFilter = string.Empty;
        PromoActiveFilter = string.Empty;
        OrderPlanFilter = string.Empty;

        ResetAddPromoForm();
    }

    public ObservableCollection<SubscriptionPlanVm> Plans { get; }
    public ObservableCollection<OrderVm> Orders { get; }
    public ObservableCollection<PromoVm> Promos { get; }

    public ObservableCollection<FilterOption> OrderStatusOptions { get; }
    public ObservableCollection<FilterOption> OrderPlanOptions { get; }
    public ObservableCollection<FilterOption> PromoTypeOptions { get; }
    public ObservableCollection<FilterOption> PromoActiveOptions { get; }

    [ObservableProperty]
    private bool _isLoadingPlans;

    [ObservableProperty]
    private bool _isLoadingOrders;

    [ObservableProperty]
    private bool _isLoadingPromos;

    [ObservableProperty]
    private string _orderSearchText = string.Empty;

    [ObservableProperty]
    private string _orderStatusFilter = string.Empty;

    [ObservableProperty]
    private string _orderPlanFilter = string.Empty;

    [ObservableProperty]
    private string _promoSearchText = string.Empty;

    [ObservableProperty]
    private string _promoTypeFilter = string.Empty;

    [ObservableProperty]
    private string _promoActiveFilter = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderCanPrev))]
    [NotifyPropertyChangedFor(nameof(OrderCanNext))]
    [NotifyPropertyChangedFor(nameof(OrderPageText))]
    private int _orderPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderCanPrev))]
    [NotifyPropertyChangedFor(nameof(OrderCanNext))]
    [NotifyPropertyChangedFor(nameof(OrderPageText))]
    private int _orderTotalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrdersTotalText))]
    private int _orderTotal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PromoCanPrev))]
    [NotifyPropertyChangedFor(nameof(PromoCanNext))]
    [NotifyPropertyChangedFor(nameof(PromoPageText))]
    private int _promoPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PromoCanPrev))]
    [NotifyPropertyChangedFor(nameof(PromoCanNext))]
    [NotifyPropertyChangedFor(nameof(PromoPageText))]
    private int _promoTotalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PromosTotalText))]
    private int _promoTotal;

    [ObservableProperty]
    private bool _isInfoOpen;

    [ObservableProperty]
    private InfoBarSeverity _infoSeverity = InfoBarSeverity.Success;

    [ObservableProperty]
    private string _infoMessage = string.Empty;

    [ObservableProperty]
    private bool _isAddPromoErrorOpen;

    [ObservableProperty]
    private string _addPromoErrorMessage = string.Empty;

    [ObservableProperty]
    private string _addPromoCode = string.Empty;

    [ObservableProperty]
    private string _addPromoDiscountType = "percentage";

    [ObservableProperty]
    private double _addPromoValue = double.NaN;

    [ObservableProperty]
    private double _addPromoMinOrderValue = double.NaN;

    [ObservableProperty]
    private double _addPromoMaxDiscount = double.NaN;

    [ObservableProperty]
    private DateTimeOffset _addPromoStartDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _addPromoEndDate = DateTimeOffset.Now.AddDays(30);

    public string PlansCountText => Plans.Count.ToString();
    public string OrdersTotalText => OrderTotal.ToString();
    public string PromosTotalText => PromoTotal.ToString();

    public bool OrderCanPrev => OrderPage > 1;
    public bool OrderCanNext => OrderPage < OrderTotalPages;
    public string OrderPageText => $"{OrderPage} / {OrderTotalPages}";

    public bool PromoCanPrev => PromoPage > 1;
    public bool PromoCanNext => PromoPage < PromoTotalPages;
    public string PromoPageText => $"{PromoPage} / {PromoTotalPages}";

    [RelayCommand]
    private async Task RefreshPlansAsync() => await LoadPlansAsync();

    [RelayCommand]
    private async Task RefreshOrdersAsync() => await LoadOrdersAsync(resetPage: false);

    [RelayCommand]
    private async Task RefreshPromosAsync() => await LoadPromosAsync(resetPage: false);

    [RelayCommand]
    private async Task ClearOrderFiltersAsync()
    {
        OrderSearchText = string.Empty;
        OrderStatusFilter = string.Empty;
        OrderPlanFilter = string.Empty;
        await LoadOrdersAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task ClearPromoFiltersAsync()
    {
        PromoSearchText = string.Empty;
        PromoTypeFilter = string.Empty;
        PromoActiveFilter = string.Empty;
        await LoadPromosAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task OrderPrevAsync()
    {
        if (!OrderCanPrev)
            return;

        OrderPage--;
        await LoadOrdersAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task OrderNextAsync()
    {
        if (!OrderCanNext)
            return;

        OrderPage++;
        await LoadOrdersAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task PromoPrevAsync()
    {
        if (!PromoCanPrev)
            return;

        PromoPage--;
        await LoadPromosAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task PromoNextAsync()
    {
        if (!PromoCanNext)
            return;

        PromoPage++;
        await LoadPromosAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task ShowAddPromoDialogAsync(ContentDialog? dialog)
    {
        if (dialog == null)
            return;

        ResetAddPromoForm();
        dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        await dialog.ShowAsync();
    }

    public async void OnOrderSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await LoadOrdersAsync(resetPage: true);
    }

    public async void OnPromoSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await LoadPromosAsync(resetPage: true);
    }

    public async void OnOrderFilterChanged(object sender, SelectionChangedEventArgs e)
        => await LoadOrdersAsync(resetPage: true);

    public async void OnPromoFilterChanged(object sender, SelectionChangedEventArgs e)
        => await LoadPromosAsync(resetPage: true);

    public async void OnAddPromoPrimaryClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        var code = (AddPromoCode ?? string.Empty).Trim();
        var discountType = (AddPromoDiscountType ?? "percentage").Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ShowAddPromoError("Code is required.");
            return;
        }

        if (double.IsNaN(AddPromoValue) || AddPromoValue <= 0)
        {
            ShowAddPromoError("Value is required and must be > 0.");
            return;
        }

        if (discountType == "percentage" && (AddPromoValue < 1 || AddPromoValue > 100))
        {
            ShowAddPromoError("Percentage value must be between 1 and 100.");
            return;
        }

        if (double.IsNaN(AddPromoMinOrderValue) || AddPromoMinOrderValue < 0)
        {
            ShowAddPromoError("Min Order Value is required and must be >= 0.");
            return;
        }

        var start = AddPromoStartDate;
        var end = AddPromoEndDate;
        if (end < start)
        {
            ShowAddPromoError("End Date must be >= Start Date.");
            return;
        }

        double? maxDiscount = null;
        if (!double.IsNaN(AddPromoMaxDiscount))
        {
            if (AddPromoMaxDiscount < 0)
            {
                ShowAddPromoError("Max Discount must be >= 0.");
                return;
            }
            maxDiscount = AddPromoMaxDiscount;
        }

        try
        {
            sender.IsPrimaryButtonEnabled = false;
            sender.IsSecondaryButtonEnabled = false;

            var body = new
            {
                code = code,
                discountType = discountType,
                value = AddPromoValue,
                minOrderValue = AddPromoMinOrderValue,
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

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadPlansAsync();
        await LoadOrdersAsync(resetPage: true);
        await LoadPromosAsync(resetPage: true);
    }

    public async Task OnNavigatedFromAsync()
    {
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
            var dto = System.Text.Json.JsonSerializer.Deserialize<ApiListResponse<PlanDto>>(rawJson, JsonOpts);

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

            OrderPlanOptions.Clear();
            OrderPlanOptions.Add(new FilterOption("All", ""));
            foreach (var p in Plans)
                OrderPlanOptions.Add(new FilterOption(p.Name, p.PlanId.ToString()));

            if (OrderPlanOptions.All(o => o.Value != OrderPlanFilter))
                OrderPlanFilter = string.Empty;
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
        if (resetPage)
            OrderPage = 1;

        try
        {
            IsLoadingOrders = true;

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = OrderPage.ToString();
            qs["limit"] = "10";

            var search = (OrderSearchText ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(search))
                qs["search"] = search;

            if (!string.IsNullOrWhiteSpace(OrderStatusFilter))
                qs["status"] = OrderStatusFilter;

            if (!string.IsNullOrWhiteSpace(OrderPlanFilter))
                qs["planId"] = OrderPlanFilter;

            var url = $"api/orders/all?{qs}";
            var httpRes = await _apiClient.GetAsync(url);

            if (!httpRes.IsSuccessStatusCode)
            {
                var raw = await httpRes.Content.ReadAsStringAsync();
                ShowToast($"Load orders failed: {raw}", InfoBarSeverity.Error);
                return;
            }

            var rawJson = await httpRes.Content.ReadAsStringAsync();
            var dto = System.Text.Json.JsonSerializer.Deserialize<ApiPaginatedResponse<OrderDto>>(rawJson, JsonOpts);

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

            OrderTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            OrderTotalPages = Math.Max(1, dto?.Pagination?.TotalPages ?? 1);

            if (OrderPage < 1) OrderPage = 1;
            if (OrderPage > OrderTotalPages) OrderPage = OrderTotalPages;
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
        if (resetPage)
            PromoPage = 1;

        try
        {
            IsLoadingPromos = true;

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = PromoPage.ToString();
            qs["limit"] = "10";

            var search = (PromoSearchText ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(search))
                qs["search"] = search;

            if (!string.IsNullOrWhiteSpace(PromoTypeFilter))
                qs["discountType"] = PromoTypeFilter;

            if (!string.IsNullOrWhiteSpace(PromoActiveFilter))
                qs["active"] = PromoActiveFilter;

            var url = $"api/promos/?{qs}";
            var httpRes = await _apiClient.GetAsync(url);

            if (!httpRes.IsSuccessStatusCode)
            {
                var raw = await httpRes.Content.ReadAsStringAsync();
                ShowToast($"Load promos failed: {raw}", InfoBarSeverity.Error);
                return;
            }

            var rawJson = await httpRes.Content.ReadAsStringAsync();
            var dto = System.Text.Json.JsonSerializer.Deserialize<ApiPaginatedResponse<PromoDto>>(rawJson, JsonOpts);

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

            PromoTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            PromoTotalPages = Math.Max(1, dto?.Pagination?.TotalPages ?? 1);

            if (PromoPage < 1) PromoPage = 1;
            if (PromoPage > PromoTotalPages) PromoPage = PromoTotalPages;
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

    private void ShowToast(string message, InfoBarSeverity severity = InfoBarSeverity.Success)
    {
        InfoSeverity = severity;
        InfoMessage = message;
        IsInfoOpen = true;
    }

    private void ResetAddPromoForm()
    {
        IsAddPromoErrorOpen = false;
        AddPromoErrorMessage = string.Empty;

        AddPromoCode = string.Empty;
        AddPromoDiscountType = "percentage";

        AddPromoValue = double.NaN;
        AddPromoMinOrderValue = double.NaN;
        AddPromoMaxDiscount = double.NaN;

        AddPromoStartDate = DateTimeOffset.Now;
        AddPromoEndDate = DateTimeOffset.Now.AddDays(30);
    }

    private void ShowAddPromoError(string message)
    {
        IsAddPromoErrorOpen = true;
        AddPromoErrorMessage = message;
    }

    private void OnPlansChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(PlansCountText));

    private void OnOrdersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(OrdersTotalText));

    private void OnPromosChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(PromosTotalText));
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
}

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
    public string OrderType { get; set; } = "";
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
