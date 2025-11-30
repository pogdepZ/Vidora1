using Microsoft.UI.Xaml;

namespace Vidora.Presentation.Gui.Contracts.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme { get; set; }

    void Initialize();
    void SetRequestedTheme();
    void SetTheme(ElementTheme theme);
}
