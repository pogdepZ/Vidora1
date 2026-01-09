using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ManageSubscriptionsViewModel : ObservableRecipient, INavigationAware
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiClient _apiClient;

    public ObservableCollection<SubscriptionPlanVm> Plans { get; } = new();
    public ObservableCollection<OrderVm> Orders { get; } = new();
    public ObservableCollection<PromoVm> Promos { get; } = new();

    public ObservableCollection<FilterOption> OrderStatusOptions { get; } = new();
    public ObservableCollection<PlanOption> OrderPlanOptions { get; } = new();
    public ObservableCollection<FilterOption> PromoTypeOptions { get; } = new();
    public ObservableCollection<FilterOption> PromoActiveOptions { get; } = new();

    [ObservableProperty]
    private FilterOption? _selectedOrderStatus;

    [ObservableProperty]
    private PlanOption? _selectedOrderPlan;

    [ObservableProperty]
    private FilterOption? _selectedPromoType;

    [ObservableProperty]
    private FilterOption? _selectedPromoActive;

    [ObservableProperty]
    private string _orderSearchText = string.Empty;

    [ObservableProperty]
    private string _promoSearchText = string.Empty;

    [ObservableProperty]
    private bool _isLoadingPlans;

    [ObservableProperty]
    private bool _isLoadingOrders;

    [ObservableProperty]
    private bool _isLoadingPromos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderCanPrev))]
    [NotifyPropertyChangedFor(nameof(OrderCanNext))]
    [NotifyPropertyChangedFor(nameof(OrderPageText))]
    private int _orderPage = 1;

    [ObservableProperty]
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
    [NotifyPropertyChangedFor(nameof(PromoCanNext))]
    [NotifyPropertyChangedFor(nameof(PromoPageText))]
    private int _promoTotalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PromosTotalText))]
    private int _promoTotal;

    [ObservableProperty]
    private bool _isInfoOpen;

    [ObservableProperty]
    private string _infoMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _infoSeverity = InfoBarSeverity.Success;

    [ObservableProperty]
    private string _infoTitle = "Success";

    private readonly int _orderLimit = 10;
    private readonly int _promoLimit = 10;

    public string PlansCountText => Plans.Count.ToString();
    public string OrdersTotalText => OrderTotal.ToString();
    public string PromosTotalText => PromoTotal.ToString();

    public bool OrderCanPrev => OrderPage > 1;
    public bool OrderCanNext => OrderPage < OrderTotalPages;
    public string OrderPageText => $"{OrderPage} / {OrderTotalPages}";

    public bool PromoCanPrev => PromoPage > 1;
    public bool PromoCanNext => PromoPage < PromoTotalPages;
    public string PromoPageText => $"{PromoPage} / {PromoTotalPages}";

    public ManageSubscriptionsViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;

        Plans.CollectionChanged += (_, __) => OnPropertyChanged(nameof(PlansCountText));

        OrderStatusOptions.Add(new FilterOption { Label = "All", Value = string.Empty });
        OrderStatusOptions.Add(new FilterOption { Label = "PENDING", Value = "PENDING" });
        OrderStatusOptions.Add(new FilterOption { Label = "PAID", Value = "PAID" });
        OrderStatusOptions.Add(new FilterOption { Label = "COMPLETED", Value = "COMPLETED" });
        OrderStatusOptions.Add(new FilterOption { Label = "CANCELLED", Value = "CANCELLED" });

        PromoTypeOptions.Add(new FilterOption { Label = "All", Value = string.Empty });
        PromoTypeOptions.Add(new FilterOption { Label = "percentage", Value = "percentage" });
        PromoTypeOptions.Add(new FilterOption { Label = "fixed_amount", Value = "fixed_amount" });

        PromoActiveOptions.Add(new FilterOption { Label = "All", Value = string.Empty });
        PromoActiveOptions.Add(new FilterOption { Label = "Active", Value = "true" });
        PromoActiveOptions.Add(new FilterOption { Label = "Inactive", Value = "false" });

        SelectedOrderStatus = OrderStatusOptions.FirstOrDefault();
        SelectedPromoType = PromoTypeOptions.FirstOrDefault();
        SelectedPromoActive = PromoActiveOptions.FirstOrDefault();
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

    public async void OrderSearchKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await LoadOrdersAsync(resetPage: true);
    }

    public async void PromoSearchKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await LoadPromosAsync(resetPage: true);
    }

    public async void OrderFilterChanged(object sender, SelectionChangedEventArgs e)
        => await LoadOrdersAsync(resetPage: true);

    public async void PromoFilterChanged(object sender, SelectionChangedEventArgs e)
        => await LoadPromosAsync(resetPage: true);

    [RelayCommand]
    private async Task RefreshPlansAsync()
        => await LoadPlansAsync();

    [RelayCommand]
    private async Task RefreshOrdersAsync()
        => await LoadOrdersAsync(resetPage: false);

    [RelayCommand]
    private async Task RefreshPromosAsync()
        => await LoadPromosAsync(resetPage: false);

    [RelayCommand]
    private async Task ClearOrderFiltersAsync()
    {
        OrderSearchText = string.Empty;
        SelectedOrderStatus = OrderStatusOptions.FirstOrDefault();
        SelectedOrderPlan = OrderPlanOptions.FirstOrDefault();
        await LoadOrdersAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task ClearPromoFiltersAsync()
    {
        PromoSearchText = string.Empty;
        SelectedPromoType = PromoTypeOptions.FirstOrDefault();
        SelectedPromoActive = PromoActiveOptions.FirstOrDefault();
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
    private async Task AddPromoAsync()
    {
        var infoBar = new InfoBar
        {
            IsOpen = false,
            IsClosable = true,
            Severity = InfoBarSeverity.Error
        };

        var codeBox = new TextBox
        {
            Header = "Code",
            PlaceholderText = "e.g. SALE25",
            MaxLength = 30
        };

        var typeCombo = new ComboBox
        {
            Header = "Discount Type",
            ItemsSource = new[] { "percentage", "fixed_amount" },
            SelectedIndex = 0
        };

        var valueBox = new NumberBox
        {
            Header = "Value",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
            Minimum = 0,
            PlaceholderText = "percentage: 1-100 | fixed_amount: >= 0"
        };

        var minOrderBox = new NumberBox
        {
            Header = "Min Order Value",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
            Minimum = 0,
            PlaceholderText = "e.g. 200000"
        };

        var maxDiscountBox = new NumberBox
        {
            Header = "Max Discount (optional)",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
            Minimum = 0,
            PlaceholderText = "e.g. 50000"
        };

        var startPicker = new DatePicker
        {
            Header = "Start Date",
            Date = DateTimeOffset.Now
        };

        var endPicker = new DatePicker
        {
            Header = "End Date",
            Date = DateTimeOffset.Now.AddDays(30)
        };

        var dateGrid = new Grid { ColumnSpacing = 12 };
        dateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
        dateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
        dateGrid.Children.Add(startPicker);
        dateGrid.Children.Add(endPicker);
        Microsoft.UI.Xaml.Controls.Grid.SetColumn(endPicker, 1);

        var stack = new StackPanel { Spacing = 12, MinWidth = 420 };
        stack.Children.Add(infoBar);
        stack.Children.Add(codeBox);
        stack.Children.Add(typeCombo);
        stack.Children.Add(valueBox);
        stack.Children.Add(minOrderBox);
        stack.Children.Add(maxDiscountBox);
        stack.Children.Add(dateGrid);

        var dialog = new ContentDialog
        {
            Title = "Add Promo Code",
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = stack,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        dialog.PrimaryButtonClick += async (sender, args) =>
        {
            args.Cancel = true;

            var code = (codeBox.Text ?? string.Empty).Trim();
            var discountType = typeCombo.SelectedItem?.ToString() ?? "percentage";

            if (string.IsNullOrWhiteSpace(code))
            {
                ShowAddPromoError(infoBar, "Code is required.");
                return;
            }

            if (double.IsNaN(valueBox.Value) || valueBox.Value <= 0)
            {
                ShowAddPromoError(infoBar, "Value is required and must be > 0.");
                return;
            }

            if (discountType == "percentage" && (valueBox.Value < 1 || valueBox.Value > 100))
            {
                ShowAddPromoError(infoBar, "Percentage value must be between 1 and 100.");
                return;
            }

            if (double.IsNaN(minOrderBox.Value) || minOrderBox.Value < 0)
            {
                ShowAddPromoError(infoBar, "Min Order Value is required and must be >= 0.");
                return;
            }

            var start = startPicker.Date;
            var end = endPicker.Date;
            if (end < start)
            {
                ShowAddPromoError(infoBar, "End Date must be >= Start Date.");
                return;
            }

            double? maxDiscount = null;
            if (!double.IsNaN(maxDiscountBox.Value))
            {
                if (maxDiscountBox.Value < 0)
                {
                    ShowAddPromoError(infoBar, "Max Discount must be >= 0.");
                    return;
                }
                maxDiscount = maxDiscountBox.Value;
            }

            try
            {
                sender.IsPrimaryButtonEnabled = false;
                sender.IsSecondaryButtonEnabled = false;

                var body = new
                {
                    code = code,
                    discountType = discountType,
                    value = valueBox.Value,
                    minOrderValue = minOrderBox.Value,
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
                ShowAddPromoError(infoBar, $"Create promo failed: {raw}");
            }
            catch (Exception ex)
            {
                ShowAddPromoError(infoBar, $"Create promo error: {ex.Message}");
            }
            finally
            {
                sender.IsPrimaryButtonEnabled = true;
                sender.IsSecondaryButtonEnabled = true;
            }
        };

        await dialog.ShowAsync();
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
                        Name = p.Name ?? string.Empty,
                        Price = p.Price,
                        Durations = p.Durations,
                        Description = p.Description ?? string.Empty
                    });
                }
            }

            OrderPlanOptions.Clear();
            OrderPlanOptions.Add(new PlanOption { Label = "All", Value = string.Empty });
            foreach (var p in Plans)
                OrderPlanOptions.Add(new PlanOption { Label = p.Name, Value = p.PlanId.ToString() });

            SelectedOrderPlan = OrderPlanOptions.FirstOrDefault();
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
            qs["limit"] = _orderLimit.ToString();

            var search = (OrderSearchText ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(search))
                qs["search"] = search;

            var status = SelectedOrderStatus?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(status))
                qs["status"] = status;

            var planId = SelectedOrderPlan?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(planId))
                qs["planId"] = planId;

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

            OrderTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            OrderTotalPages = dto?.Pagination?.TotalPages ?? 1;
            if (OrderTotalPages < 1)
                OrderTotalPages = 1;

            if (OrderPage < 1)
                OrderPage = 1;
            if (OrderPage > OrderTotalPages)
                OrderPage = OrderTotalPages;
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
            qs["limit"] = _promoLimit.ToString();

            var search = (PromoSearchText ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(search))
                qs["search"] = search;

            var type = SelectedPromoType?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(type))
                qs["discountType"] = type;

            var active = SelectedPromoActive?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(active))
                qs["active"] = active;

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

            PromoTotal = dto?.Pagination?.Total ?? (dto?.Data?.Length ?? 0);
            PromoTotalPages = dto?.Pagination?.TotalPages ?? 1;
            if (PromoTotalPages < 1)
                PromoTotalPages = 1;

            if (PromoPage < 1)
                PromoPage = 1;
            if (PromoPage > PromoTotalPages)
                PromoPage = PromoTotalPages;
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
        InfoTitle = severity == InfoBarSeverity.Success ? "Success" : "Error";
        IsInfoOpen = true;
    }

    private static void ShowAddPromoError(InfoBar infoBar, string message)
    {
        infoBar.Severity = InfoBarSeverity.Error;
        infoBar.Message = message;
        infoBar.IsOpen = true;
    }
}

public sealed class FilterOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class PlanOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
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
