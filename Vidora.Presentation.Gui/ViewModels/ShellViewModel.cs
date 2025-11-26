using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Events;
using Vidora.Presentation.Gui.Contracts.Services;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private object? _selectedItem;
    public object? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    private bool _isBackEnabled;
    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    private bool _isBackButtonVisible = false;
    public bool IsBackButtonVisible
    {
        get => _isBackButtonVisible;
        set => SetProperty(ref _isBackButtonVisible, value);
    }

    private bool _isPaneToggleButtonVisible = false;
    public bool IsPaneToggleButtonVisible
    {
        get => _isPaneToggleButtonVisible;
        set => SetProperty(ref _isPaneToggleButtonVisible, value);
    }

    private NavigationViewPaneDisplayMode _paneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
    public NavigationViewPaneDisplayMode PaneDisplayMode
    {
        get => _paneDisplayMode;
        set => SetProperty(ref _paneDisplayMode, value);
    }

    public INavigationViewService NavigationViewService { get; }
    private readonly IPageService _pageService;
    private readonly INavigationService _navigationService;
    private readonly ISessionStateService _sessionState;
    public ShellViewModel(
        INavigationViewService navigationViewService,
        IPageService pageService,
        INavigationService navigationService,
        ISessionStateService sessionService
        )
    {
        NavigationViewService = navigationViewService;

        _pageService = pageService;

        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        _sessionState = sessionService;
        _sessionState.SessionChanged += OnSessionChanged;
    }

    private async void OnSessionChanged(object? sender, SessionChangeEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionChangeReason.AutoRestore:
            case SessionChangeReason.ManualLogin:
                await _navigationService.NavigateToAsync<SettingsViewModel>(clearNavigation: true);
                break;
            case SessionChangeReason.ManualLogout:
            case SessionChangeReason.ForcedLogout:
            case SessionChangeReason.SessionExpired:
                await _navigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
                break;
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = _navigationService.CanGoBack;

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

        if (e.SourcePageType == _pageService.GetPageType<LoginViewModel>() ||
            e.SourcePageType == _pageService.GetPageType<SplashViewModel>())
        {
            IsBackButtonVisible = false;
            IsPaneToggleButtonVisible = false;
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
        }
        else
        {
            IsBackButtonVisible = true;
            IsPaneToggleButtonVisible = true;
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        }
    }
}