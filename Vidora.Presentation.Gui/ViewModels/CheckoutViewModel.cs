using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class CheckoutViewModel : ObservableRecipient, INavigationAware
{
    private readonly CreateOrderUseCase _createOrderUseCase;
    private readonly GetAvailablePromosUseCase _getAvailablePromosUseCase;
    private readonly ApplyDiscountUseCase _applyDiscountUseCase;
    private readonly ConfirmPaymentUseCase _confirmPaymentUseCase;
    private readonly IMapper _mapper;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCreatingOrder;

    [ObservableProperty]
    private bool _isApplyingDiscount;

    [ObservableProperty]
    private bool _isConfirmingPayment;

    [ObservableProperty]
    private bool _showPaymentScreen;

    [ObservableProperty]
    private bool _paymentSuccess;

    [ObservableProperty]
    private SubscriptionPlan? _selectedPlan;

    [ObservableProperty]
    private Order? _currentOrder;

    [ObservableProperty]
    private Promo? _selectedPromo;

    [ObservableProperty]
    private bool _hasNoPromos = true;

    [ObservableProperty]
    private bool _isPromosLoaded;

    public ObservableCollection<Promo> AvailablePromos { get; } = [];

    public CheckoutViewModel(
        CreateOrderUseCase createOrderUseCase,
        GetAvailablePromosUseCase getAvailablePromosUseCase,
        ApplyDiscountUseCase applyDiscountUseCase,
        ConfirmPaymentUseCase confirmPaymentUseCase,
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _createOrderUseCase = createOrderUseCase;
        _getAvailablePromosUseCase = getAvailablePromosUseCase;
        _applyDiscountUseCase = applyDiscountUseCase;
        _confirmPaymentUseCase = confirmPaymentUseCase;
        _mapper = mapper;
        _infoBarService = infoBarService;
        _navigationService = navigationService;

        // Subscribe to collection changes to update HasNoPromos
        AvailablePromos.CollectionChanged += OnAvailablePromosChanged;
    }

    private void OnAvailablePromosChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasNoPromos = AvailablePromos.Count == 0;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        if (parameter is SubscriptionPlan plan)
        {
            SelectedPlan = plan;
            await InitializeCheckoutAsync();
        }
        else
        {
            _infoBarService.ShowError("Không tìm thấy thông tin gói đăng ký");
            await _navigationService.NavigateToAsync<SubscriptionViewModel>();
        }
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    private async Task InitializeCheckoutAsync()
    {
        if (SelectedPlan == null) return;

        try
        {
            IsLoading = true;
            IsCreatingOrder = true;

            // Create order
            var orderResult = await _createOrderUseCase.ExecuteAsync(SelectedPlan.PlanId);
            if (orderResult.IsFailure)
            {
                _infoBarService.ShowError(orderResult.Error);
                return;
            }

            CurrentOrder = _mapper.Map<Order>(orderResult.Value.Order);

            // Load available promos
            await LoadPromosAsync();
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi khi tạo đơn hàng: {ex.Message}");
        }
        finally
        {
            IsCreatingOrder = false;
            IsLoading = false;
        }
    }

    private async Task LoadPromosAsync()
    {
        try
        {
            var promosResult = await _getAvailablePromosUseCase.ExecuteAsync();
 
            if (promosResult.IsSuccess)
            {
                AvailablePromos.Clear();
                foreach (var promo in promosResult.Value.Promos)
                {
                    // Only show promos that meet minimum order value
                    if (CurrentOrder != null && CurrentOrder.OriginalAmount >= promo.MinOrderValue)
                    {
                        var mappedPromo = _mapper.Map<Promo>(promo);
             
                        AvailablePromos.Add(mappedPromo);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading promos: {ex.Message}");
        }
        finally
        {
            IsPromosLoaded = true;
        }
    }

    [RelayCommand]
    private async Task ApplyPromoAsync()
    {
        if (CurrentOrder == null || SelectedPromo == null) return;

        try
        {
            IsApplyingDiscount = true;

            var result = await _applyDiscountUseCase.ExecuteAsync(CurrentOrder.OrderId, SelectedPromo.DiscountId);
            if (result.IsFailure)
            {
                _infoBarService.ShowError(result.Error);
                return;
            }

            CurrentOrder = _mapper.Map<Order>(result.Value.Order);
            _infoBarService.ShowSuccess("Đã áp dụng mã giảm giá");
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi áp dụng mã giảm giá: {ex.Message}");
        }
        finally
        {
            IsApplyingDiscount = false;
        }
    }

    [RelayCommand]
    private void ProceedToPayment()
    {
        if (CurrentOrder == null) return;
        ShowPaymentScreen = true;
    }

    [RelayCommand]
    private async Task ConfirmPaymentAsync()
    {
        if (CurrentOrder == null) return;

        try
        {
            IsConfirmingPayment = true;

            var result = await _confirmPaymentUseCase.ExecuteAsync(CurrentOrder.OrderId);
            if (result.IsFailure)
            {
                _infoBarService.ShowError(result.Error);
                return;
            }

            PaymentSuccess = true;
            _infoBarService.ShowSuccess("Thanh toán thành công!");
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi xác nhận thanh toán: {ex.Message}");
        }
        finally
        {
            IsConfirmingPayment = false;
        }
    }

    [RelayCommand]
    private async Task BackToSubscriptionsAsync()
    {
        await _navigationService.NavigateToAsync<SubscriptionViewModel>();
    }

    [RelayCommand]
    private void CancelPayment()
    {
        ShowPaymentScreen = false;
    }
}
