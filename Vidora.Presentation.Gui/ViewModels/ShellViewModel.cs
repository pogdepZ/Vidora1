using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
    public ShellViewModel(
        INavigationViewService navigationViewService,
        IPageService pageService,
        INavigationService navigationService
        )
    {
        NavigationViewService = navigationViewService;
        _pageService = pageService;
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = _navigationService.CanGoBack;

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            SelectedItem = selectedItem;
        }
    }
}