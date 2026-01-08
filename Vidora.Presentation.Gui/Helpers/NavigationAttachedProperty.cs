using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Vidora.Presentation.Gui.Helpers;

public class NavigationAttachedProperty
{
    public static string GetNavigateTo(NavigationViewItem item)
        => (string)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(NavigationViewItem item, string value)
        => item.SetValue(NavigateToProperty, value);

    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(NavigationAttachedProperty), new PropertyMetadata(null));
}
