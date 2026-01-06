using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SubscriptionViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetSubscriptionPlansUseCase _getSubscriptionPlansUseCase;
    private readonly GetMySubscriptionsUseCase _getMySubscriptionsUseCase;
    private readonly IMapper _mapper;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<SubscriptionPlan> Plans { get; } = [];

    public SubscriptionViewModel(
        GetSubscriptionPlansUseCase getSubscriptionPlansUseCase,
        GetMySubscriptionsUseCase getMySubscriptionsUseCase,
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _getSubscriptionPlansUseCase = getSubscriptionPlansUseCase;
        _getMySubscriptionsUseCase = getMySubscriptionsUseCase;
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

            // Fetch plans and my subscriptions in parallel
            var plansTask = _getSubscriptionPlansUseCase.ExecuteAsync();
            var mySubscriptionsTask = _getMySubscriptionsUseCase.ExecuteAsync();

            await Task.WhenAll(plansTask, mySubscriptionsTask);

            var plansResult = await plansTask;
            var mySubscriptionsResult = await mySubscriptionsTask;

            if (plansResult.IsFailure)
            {
                _infoBarService.ShowError(plansResult.Error);
                return;
            }

            // Get purchased plan IDs
            var purchasedPlanIds = new HashSet<int>();
            if (mySubscriptionsResult.IsSuccess)
            {
                foreach (var sub in mySubscriptionsResult.Value.Subscriptions)
                {
                    purchasedPlanIds.Add(sub.PlanId);
                }
            }

            // Map plans and set IsPurchased
            Plans.Clear();
            foreach (var plan in plansResult.Value.Plans)
            {
                var mappedPlan = _mapper.Map<SubscriptionPlan>(plan);
                mappedPlan.IsPurchased = purchasedPlanIds.Contains(plan.PlanId);
                Plans.Add(mappedPlan);
            }
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi tải danh sách gói: {ex.Message}");
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
}