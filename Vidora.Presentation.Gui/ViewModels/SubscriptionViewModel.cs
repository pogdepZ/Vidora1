using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;
namespace Vidora.Presentation.Gui.ViewModels;

public partial class SubscriptionViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetSubscriptionPlansUseCase _getSubscriptionPlansUseCase;
    private readonly GetCurrentSubscriptionUseCase _getCurrentSubscriptionUseCase;
    private readonly IMapper _mapper;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasActiveSubscription;

    [ObservableProperty]
    private int _currentPlanId;

    [ObservableProperty]
    private int _leftDay;

    public ObservableCollection<SubscriptionPlan> Plans { get; } = [];

    public SubscriptionViewModel(
        GetSubscriptionPlansUseCase getSubscriptionPlansUseCase,
        GetCurrentSubscriptionUseCase getCurrentSubscriptionUseCase,
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _getSubscriptionPlansUseCase = getSubscriptionPlansUseCase;
        _getCurrentSubscriptionUseCase = getCurrentSubscriptionUseCase;
        _mapper = mapper;
        _infoBarService = infoBarService;
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadPlansAsync();
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LoadPlansAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;

            // Fetch plans and current subscription in parallel
            var plansTask = _getSubscriptionPlansUseCase.ExecuteAsync();
            var currentSubscriptionTask = _getCurrentSubscriptionUseCase.ExecuteAsync();

            await Task.WhenAll(plansTask, currentSubscriptionTask);

            var plansResult = await plansTask;
            var currentSubscriptionResult = await currentSubscriptionTask;

            if (plansResult.IsFailure)
            {
                await _infoBarService.ShowErrorAsync(plansResult.Error);
                return;
            }

            // Check if user has active subscription
            HasActiveSubscription = false;
            CurrentPlanId = 0;
            LeftDay = 0;

            if (currentSubscriptionResult.IsSuccess && currentSubscriptionResult.Value.HasActiveSubscription)
            {
                var subscription = currentSubscriptionResult.Value.Subscription!;
                HasActiveSubscription = true;
                CurrentPlanId = subscription.PlanId;
                LeftDay = subscription.LeftDay;
            }

            // Map plans and set IsPurchased based on current subscription
            Plans.Clear();
            foreach (var plan in plansResult.Value.Plans)
            {
                var mappedPlan = _mapper.Map<SubscriptionPlan>(plan);
                // Mark as purchased if this is the current active plan
                mappedPlan.IsPurchased = HasActiveSubscription && plan.PlanId == CurrentPlanId;
                Plans.Add(mappedPlan);
            }
        }
        catch (Exception ex)
        {
            await _infoBarService.ShowErrorAsync($"Lỗi tải danh sách gói: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GetPlanAsync(SubscriptionPlan plan)
    {
        if (plan == null) {
            return;
        }
        await _navigationService.NavigateToAsync<CheckoutViewModel>(plan);
    }

    [RelayCommand]
    private async Task RenewPlanAsync(SubscriptionPlan plan)
    {
        if (plan == null)
        {
            return;
        }
        // Navigate to checkout for renewal - same flow as new subscription
        await _navigationService.NavigateToAsync<CheckoutViewModel>(plan);
    }
}