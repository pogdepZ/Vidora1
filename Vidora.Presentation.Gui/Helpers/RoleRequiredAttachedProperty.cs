using Microsoft.UI.Xaml;

namespace Vidora.Presentation.Gui.Helpers;

public static class RoleRequiredAttachedProperty
{
    public static readonly DependencyProperty RoleRequiredProperty =
        DependencyProperty.RegisterAttached(
            "RoleRequired",
            typeof(string),
            typeof(RoleRequiredAttachedProperty),
            new PropertyMetadata(null));

    public static void SetRoleRequired(DependencyObject obj, string value)
        => obj.SetValue(RoleRequiredProperty, value);

    public static string GetRoleRequired(DependencyObject obj)
        => (string)obj.GetValue(RoleRequiredProperty);
}