using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.ViewModels;
using Vidora.Presentation.Gui.Views;

namespace Vidora.Presentation.Gui.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<SplashViewModel, SplashPage>();
        Configure<LoginViewModel, LoginPage>();

        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType<TViewModel>()
        where TViewModel : class
    {
        return GetPageType(typeof(TViewModel).Name);
    }

    public Type GetPageType(string key)
    {
        lock (_pages)
        {
            if (_pages.TryGetValue(key, out var pageType))
            {
                return pageType;
            }

            throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
        }
    }

    private void Configure<VM, V>()
        where VM : class
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).Name;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type {type.Name} is already configured");
            }

            _pages.Add(key, type);
        }
    }
}