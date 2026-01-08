using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Extensions;

namespace Vidora.Presentation.Gui.Services;

public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private Frame? _frame;
    private object? _lastParameterUsed;

    public event NavigatedEventHandler? Navigated;

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public Frame? Frame
    {
        get => _frame;
        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public async Task<bool> NavigateToAsync<TViewModel>(object? parameter = null, bool clearNavigation = false) 
        where TViewModel : class
    {
        var pageKey = typeof(TViewModel).Name;

        return await NavigateToAsync(pageKey, parameter, clearNavigation);
    }

    public async Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        var isDifferentPage = _frame?.Content?.GetType() != pageType;
        var isDifferentParameter = parameter != null && !parameter.Equals(_lastParameterUsed);

        if (_frame != null && (isDifferentPage || isDifferentParameter))
        {
            _frame.Tag = clearNavigation;

            var vmBeforeNavigation = _frame.GetPageViewModel();
            var navigated = _frame.Navigate(pageType, parameter);

            if (navigated)
            {
                _lastParameterUsed = parameter;

                if (vmBeforeNavigation is INavigationAware navigationAware)
                    await navigationAware.OnNavigatedFromAsync();
            }

            return navigated;
        }

        return false;
    }

    private void RegisterFrameEvents()
    {
        if (_frame != null)
            _frame.Navigated += OnNavigated;
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null)
            _frame.Navigated -= OnNavigated;
    }

    private async void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is not Frame frame)
            return;

        if (frame.Tag is bool clearNavigation && clearNavigation)
            frame.BackStack.Clear();

        if (frame.GetPageViewModel() is INavigationAware navigationAware)
            await navigationAware.OnNavigatedToAsync(e.Parameter);

        Navigated?.Invoke(sender, e);
    }

    public async Task<bool> GoBackAsync()
    {
        if (!CanGoBack)
            return false;

        var vmBeforeNavigation = _frame.GetPageViewModel();
        _frame.GoBack();

        if (vmBeforeNavigation is INavigationAware navigationAware)
            await navigationAware.OnNavigatedFromAsync();

        return true;
    }

    public void SetListDataItemForNextConnectedAnimation(object item) => Frame?.SetListDataItemForNextConnectedAnimation(item);
}
