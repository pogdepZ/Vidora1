using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using Vidora.Presentation.Gui.Helpers;

namespace Vidora.Presentation.Gui.Behaviors;

public class NavigationViewRoleFilterBehavior : Behavior<NavigationView>
{
    public string? CurrentRole
    {
        get => (string?)GetValue(CurrentRoleProperty);
        set => SetValue(CurrentRoleProperty, value);
    }

    public static readonly DependencyProperty CurrentRoleProperty =
        DependencyProperty.Register(
            nameof(CurrentRole),
            typeof(string),
            typeof(NavigationViewRoleFilterBehavior),
            new PropertyMetadata(null, OnRoleChanged));

    private static void OnRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NavigationViewRoleFilterBehavior behavior || behavior.AssociatedObject is not NavigationView nav)
            return;

        behavior.Refresh(nav, e.NewValue as string);
    }

    private void Refresh(NavigationView nav, string? role)
    {
        foreach (var item in nav.MenuItems)
        {
            if (item is not NavigationViewItem navItem) continue;

            var requiredRole = RoleRequiredAttachedProperty.GetRoleRequired(navItem);

            if (string.IsNullOrWhiteSpace(requiredRole))
            {
                navItem.Visibility = Visibility.Visible;
                continue;
            }

            navItem.Visibility =
                string.Equals(requiredRole, role, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
