using Microsoft.UI.Xaml;
using System;
using Vidora.Core.Interfaces.Storage;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Helpers;

namespace Vidora.Presentation.Gui.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string ThemeSettingsKey = "AppRequestedTheme";

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    private readonly ILocalSettingsService _localSetting;
    public ThemeSelectorService(ILocalSettingsService localSetting)
    {
        _localSetting = localSetting;
    }

    public void Initialize()
    {
        var themeName = _localSetting.ReadSettings<string>(ThemeSettingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            Theme = cacheTheme;
        }
        else
        {
            Theme = ElementTheme.Default;
        }
    }

    public void SetTheme(ElementTheme theme)
    {
        Theme = theme;

        SetRequestedTheme();

        _localSetting.SaveSettings(ThemeSettingsKey, theme.ToString());
    }

    public void SetRequestedTheme()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            TitleBarHelper.UpdateTitleBar(Theme);
        }
    }
}
