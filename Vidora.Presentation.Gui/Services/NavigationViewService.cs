using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Helpers;

namespace Vidora.Presentation.Gui.Services;

public class NavigationViewService : INavigationViewService
{
    private readonly INavigationService _navigationService;

    private readonly IPageService _pageService;

    private NavigationView? _navigationView;

    public object? SettingsItem => _navigationView?.SettingsItem;

    public NavigationViewService(INavigationService navigationService, IPageService pageService)
    {
        _navigationService = navigationService;
        _pageService = pageService;
    }

    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationService.Frame = _navigationView.Content as Frame ?? new Frame();
        _navigationView.BackRequested += OnBackRequested;
        _navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
        if (_navigationView != null)
        {
            _navigationView.BackRequested -= OnBackRequested;
            _navigationView.ItemInvoked -= OnItemInvoked;
        }
    }

    private async void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        => await _navigationService.GoBackAsync();

    private async void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationAttachedProperty.NavigateToProperty) is string pageKey)
            {
                await _navigationService.NavigateToAsync(pageKey);
            }
        }
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        if (_navigationView != null)
        {
            return GetSelectedItem(_navigationView.MenuItems, pageType) ?? GetSelectedItem(_navigationView.FooterMenuItems, pageType);
        }

        return null;
    }

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (var item in menuItems.OfType<NavigationViewItem>())
        {
            if (IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            var selectedChild = GetSelectedItem(item.MenuItems, pageType);
            if (selectedChild != null)
            {
                return selectedChild;
            }
        }

        return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        if (menuItem.GetValue(NavigationAttachedProperty.NavigateToProperty) is not string pageKey)
            return false;

        try
        {
            return _pageService.GetPageType(pageKey) == sourcePageType;
        }
        catch
        {
            return false;
        }
    }
}
