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
    private Role? _currentUserRole;
    public Role? CurrentUserRole
    {
        get => _currentUserRole;
        set => SetProperty(ref _currentUserRole, value);
    }

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

    public IInfoBarService InfoBarService { get; }
    public INavigationViewService NavigationViewService { get; }

    private readonly IPageService _pageService;
    private readonly INavigationService _navigationService;
    private readonly ISessionStateService _sessionState;
    private readonly IMapper _mapper;
    public ShellViewModel(
        IInfoBarService infoBarService,
        INavigationViewService navigationViewService,
        IPageService pageService,
        INavigationService navigationService,
        ISessionStateService sessionService,
        IMapper mapper
        )
    {
        NavigationViewService = navigationViewService;
        InfoBarService = infoBarService;

        _pageService = pageService;

        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        _sessionState = sessionService;
        _sessionState.SessionChanged += OnSessionChanged;

        _mapper = mapper;
    }

    private async void OnSessionChanged(object? sender, SessionChangeEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionChangeReason.AutoRestore:
            case SessionChangeReason.ManualLogin:
                IsBackButtonVisible = true;
                IsPaneToggleButtonVisible = true;
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                await _navigationService.NavigateToAsync<SettingsViewModel>(clearNavigation: true);
                break;
            case SessionChangeReason.ManualLogout:
            case SessionChangeReason.ForcedLogout:
            case SessionChangeReason.SessionExpired:
                IsBackButtonVisible = false;
                IsPaneToggleButtonVisible = false;
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
                await _navigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
                break;
        }

        CurrentUserRole = _mapper.Map<Role?>(_sessionState.CurrentUser?.Role);
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

        InfoBarService.CloseIfOpen();
    }
}