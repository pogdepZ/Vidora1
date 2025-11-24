using Microsoft.UI.Xaml.Controls;
using System;

namespace Vidora.Presentation.Gui.Contracts.Services;

public interface INavigationViewService
{
    object? SettingsItem { get; }

    NavigationViewItem? GetSelectedItem(Type pageType);
    
    void Initialize(NavigationView navigationView);

    void UnregisterEvents();
}
