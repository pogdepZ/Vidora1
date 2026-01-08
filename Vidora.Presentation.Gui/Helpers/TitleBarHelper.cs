using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Vidora.Presentation.Gui.Helpers;

public static class TitleBarHelper
{
    public static void UpdateTitleBar(ElementTheme theme)
    {
        if (App.MainWindow.ExtendsContentIntoTitleBar)
        {
            // If the theme is Default, determine the actual theme based on the app's requested theme
            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Light
                    ? ElementTheme.Light
                    : ElementTheme.Dark;
            }

            var titleBar = App.MainWindow.AppWindow.TitleBar;

            titleBar.ButtonForegroundColor = theme == ElementTheme.Dark ? Colors.White : Colors.DarkGray;
            titleBar.BackgroundColor = Colors.Transparent;

            titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            titleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }
}