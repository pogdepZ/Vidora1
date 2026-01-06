using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Events;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Role? _currentUserRole;

    [ObservableProperty]
    private object? _selectedItem;

    [ObservableProperty]
    private bool _isBackEnabled;

    [ObservableProperty]
    private bool _isBackButtonVisible = false;

    [ObservableProperty]
    private bool _isPaneToggleButtonVisible = false;

    [ObservableProperty]
    private NavigationViewPaneDisplayMode _paneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;

    public IInfoBarService InfoBarService { get; }
    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }

    private readonly IPageService _pageService;
    private readonly ISessionStateService _sessionState;
    private readonly IMapper _mapper;
    public ShellViewModel(
        IInfoBarService infoBarService,
        INavigationService navigationService,
        INavigationViewService navigationViewService,
        IPageService pageService,
        ISessionStateService sessionService,
        IMapper mapper
        )
    {
        NavigationViewService = navigationViewService;
        InfoBarService = infoBarService;

        _pageService = pageService;

        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        _sessionState = sessionService;
        _sessionState.SessionChanged += OnSessionChanged;

        _mapper = mapper;
    }

    private async void OnSessionChanged(object? sender, SessionChangeEventArgs e)
    {
        CurrentUserRole = _mapper.Map<Role?>(_sessionState.CurrentUser?.Role);

        switch (e.Reason)
        {
            case SessionChangeReason.AutoRestore:
            case SessionChangeReason.ManualLogin:
                IsBackButtonVisible = true;
                IsPaneToggleButtonVisible = true;
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                if (CurrentUserRole == Role.User)
                {
                    await NavigationService.NavigateToAsync<HomeViewModel>(clearNavigation: true);
                }
                else
                {
                    await NavigationService.NavigateToAsync<AdminDashboardViewModel>(clearNavigation: true);
                }
                break;
            case SessionChangeReason.ManualLogout:
            case SessionChangeReason.ForcedLogout:
            case SessionChangeReason.SessionExpired:
                IsBackButtonVisible = false;
                IsPaneToggleButtonVisible = false;
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
                await NavigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
                break;
        }
    }

    private async void OnNavigated(object sender, NavigationEventArgs e)
    {
        await InfoBarService.CloseAsync();

        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == _pageService.GetPageType<SettingsViewModel>())
        {
            SelectedItem = NavigationViewService.SettingsItem;
        }
        else
        {
            var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
            if (selectedItem != null)
            {
                SelectedItem = selectedItem;
            }
        }

        if (CurrentUserRole != null)
        {
            if (e.SourcePageType == _pageService.GetPageType<VideoPlayerViewModel>())
            {
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
                if (IsBackEnabled)
                {
                    IsPaneToggleButtonVisible = false;
                }
                else
                {
                    IsBackButtonVisible = false;
                    IsPaneToggleButtonVisible = true;
                }
            }
            else if (PaneDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal)
            {
                IsBackButtonVisible = true;
                IsPaneToggleButtonVisible = true;
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
            }
        }
    }
}